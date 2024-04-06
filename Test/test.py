import subprocess
import contextlib
import json
import joblib
import pathlib
import os
import tqdm
import multiprocessing

@contextlib.contextmanager
def tqdm_joblib(tqdm_object):
    """Context manager to patch joblib to report into tqdm progress bar given as argument"""

    class TqdmBatchCompletionCallback(joblib.parallel.BatchCompletionCallBack):
        def __init__(self, *args, **kwargs):
            super().__init__(*args, **kwargs)

        def __call__(self, *args, **kwargs):
            tqdm_object.update(n=self.batch_size)
            return super().__call__(*args, **kwargs)

    old_batch_callback = joblib.parallel.BatchCompletionCallBack
    joblib.parallel.BatchCompletionCallBack = TqdmBatchCompletionCallback
    try:
        yield tqdm_object
    finally:
        joblib.parallel.BatchCompletionCallBack = old_batch_callback
        tqdm_object.close()


def EnsureFolder(folder: pathlib.Path) -> pathlib.Path:
    if not folder.is_dir():
        os.makedirs(str(folder), exist_ok=True)
    return folder

script_file_path = pathlib.Path(__file__).parent
build_path = script_file_path.parent / 'Builds' / 'WindowsScenarioOnly' / 'Kaede2.exe'
master_data_path = script_file_path.parent / 'Assets' / '_Kaede2Assets' / 'Resources' / 'master_data' / 'MasterScenarioInfo.masterdata'
result_path = script_file_path / 'results.txt'
log_folder = EnsureFolder(script_file_path / 'logs')

failed_test = []
failed_reason = ["Passed", "BadParameter", "Exception", "ResourceNotFound", "NotImplemented"]

# jobs = multiprocessing.cpu_count()
jobs = 1 # addressables have problem with mutiple instances

def Test(case : str):
    global failed_test

    log_path = log_folder / f'{case}.txt'

    params = [str(build_path), '-logfile', str(log_path), '-test-mode', '-scenario', case]
    if jobs > 1:
        params.append('-batchmode')
        params.append('-nographics')
    process = subprocess.Popen(params, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    process.wait()
    return_code = process.returncode

    if return_code == 0:
        log_path.unlink()
    else:
        failed_test.append((case, return_code))

master_data = json.loads(master_data_path.read_text(encoding='utf-8'))
test_cases = [scenario['ScenarioName'] for scenario in master_data['scenarioInfo']]

with tqdm_joblib(tqdm.tqdm(test_cases)):
    joblib.Parallel(n_jobs=(jobs), backend="threading")(joblib.delayed(Test)(c) for c in test_cases)

with open(result_path, 'w') as f:
    for case, return_code in failed_test:
        print(f"Test case {case} failed with code {return_code} ({failed_reason[return_code]})")
        f.write(f"{case}\t{return_code}\n")
