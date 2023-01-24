using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SonarPlugin.Utility;

namespace SonarPlugin.Config
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility")]
    public sealed class SonarConfigurationColors
    {
        public Vector4 HuntHealthy = ColorPalette.Green;
        public Vector4 HuntPulled = new(1.0f, 0.6f, 0.0f, 1.0f);
        public Vector4 HuntDead = ColorPalette.Red;

        public Vector4 FateRunning = new(0.0f, 0.6f, 1.0f, 1.0f);
        public Vector4 FateProgress = new(0.0f, 0.8f, 1.0f, 1.0f);
        public Vector4 FateComplete = new(0.0f, 1.0f, 1.0f, 1.0f);
        public Vector4 FateFailed = ColorPalette.Red;
        public Vector4 FatePreparation = ColorPalette.White;
        public Vector4 FateUnknown = new(0.467f, 0.467f, 0.467f, 1.0f);

        public void SetDefaults()
        {
            this.HuntHealthy = ColorPalette.Green;
            this.HuntPulled = new(1.0f, 0.6f, 0.0f, 1.0f);
            this.HuntDead = ColorPalette.Red;

            this.FateRunning = new(0.0f, 0.6f, 1.0f, 1.0f);
            this.FateProgress = new(0.0f, 0.8f, 1.0f, 1.0f);
            this.FateComplete = new(0.0f, 1.0f, 1.0f, 1.0f);
            this.FateFailed = ColorPalette.Red;
            this.FatePreparation = ColorPalette.White;
            this.FateUnknown = new(0.467f, 0.467f, 0.467f, 1.0f);
        }
    }
}
