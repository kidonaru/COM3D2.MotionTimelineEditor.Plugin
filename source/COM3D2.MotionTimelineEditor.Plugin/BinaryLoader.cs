using System;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public static class BinaryLoader
    {
        private static byte[] _fileBuffer;

        public static void ClearCache()
        {
            _fileBuffer = null;
        }

        public static byte[] ReadAFileBase(string filename)
        {
            try
            {
                byte[] result;
                using (AFileBase afileBase = GameUty.FileOpen(filename, null))
                {
                    if (afileBase == null || !afileBase.IsValid() || afileBase.GetSize() == 0)
                    {
                        PluginUtils.LogWarning("AFileBase '" + filename + "' not found");
                        result = null;
                    }
                    else
                    {
                        if (_fileBuffer == null)
                        {
                            _fileBuffer = new byte[Math.Max(500000L, afileBase.GetSize())];
                        }
                        else if ((long)_fileBuffer.Length < afileBase.GetSize())
                        {
                            _fileBuffer = new byte[afileBase.GetSize()];
                        }
                        afileBase.Read(ref _fileBuffer, afileBase.GetSize());
                        result = _fileBuffer;
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.LogError("Could not read file '" + filename + "'");
                return null;
            }
        }
    }
}