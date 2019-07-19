using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.VersionControl
{
    public struct Version
    {
        public int Major;
        public int Minor;
        public int Build;
        public bool HasVersion;

        public Version(int major, int minor, int build)
        {
            Major = major;
            Minor = minor;
            Build = build;
            HasVersion = true;
        }

        public Version(int build)
        {
            Major = 0;
            Minor = 0;
            Build = build;
            HasVersion = false;
        }

        public override string ToString()
        {
            return Major + "." + Minor + " (" + Build + ")";
        }
    }
}
