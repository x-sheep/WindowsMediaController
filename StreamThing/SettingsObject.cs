using System;
using System.Collections.Generic;
using System.Text;

namespace StreamThing
{
    public class SettingsObject
    {
        public int? Width { get; set; } = null;

        public MediaSourceVisibilty MediaSourceVisibilty { get; set; } = MediaSourceVisibilty.Always;
    }

    public enum MediaSourceVisibilty
    {
        Always,
        WhenPlaying,
        Never
    }
}
