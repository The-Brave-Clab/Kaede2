using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kaede2.Live2D
{
    // ReSharper disable InconsistentNaming
    public class Live2DModelJson
    {
        public string version { get; set; } = "";
        public string model { get; set; } = "";
        public string[] textures { get; set; } = null;

        public class MotionFile
        {
            public string file { get; set; } = "";
        }

        public Dictionary<string, List<MotionFile>> motions { get; set; } = null;
        public string pose { get; set; } = "";

        public static Live2DModelJson FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Live2DModelJson>(json);
        }
    }
    // ReSharper restore InconsistentNaming
}