#import <Foundation/Foundation.h>
#import <AppKit/AppKit.h>

#include <string>

extern "C" void OpenMacSaveFileDialog(const char* title, const char* directory, const char* filename, void (*callback)(const char*))
{
    std::string titleStr = title;
    std::string directoryStr = directory;
    std::string filenameStr = filename;

    dispatch_async(dispatch_get_main_queue(), ^{
        @autoreleasepool {
            NSSavePanel* savePanel = [NSSavePanel savePanel];
            [savePanel setTitle:[NSString stringWithUTF8String:titleStr.c_str()]];
            [savePanel setDirectoryURL:[NSURL fileURLWithPath:[NSString stringWithUTF8String:directoryStr.c_str()]]];
            [savePanel setNameFieldStringValue:[NSString stringWithUTF8String:filenameStr.c_str()]];

            [savePanel beginWithCompletionHandler:^(NSInteger result) {
                if (result == NSModalResponseOK) {
                    NSString *selectedFile = [[savePanel URL] path];
                    // Use the callback to return the selected file path
                    callback([selectedFile UTF8String]);
                } else {
                    // Use the callback to indicate cancellation or failure
                    callback(nullptr);
                }
            }];
        }
    });
}
