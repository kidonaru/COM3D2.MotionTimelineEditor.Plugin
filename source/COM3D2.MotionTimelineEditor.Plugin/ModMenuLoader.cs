using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MenuInfo
    {
        public string name;
        public string modelFileName;
    }

    public class ModMenuLoader
    {
        private static byte[] fileBuffer;
        private static Dictionary<string, MenuInfo> menuCache = new Dictionary<string, MenuInfo>();

        public static void ClearCache()
        {
            fileBuffer = null;
            menuCache.Clear();
        }

        public static MenuInfo Load(string menuFileName)
        {
            MenuInfo menu;
            if (menuCache.TryGetValue(menuFileName, out menu))
            {
                return menu;
            }

            menu = LoadInternal(menuFileName);
            menuCache[menuFileName] = menu;

            return menu;
        }

        private static MenuInfo LoadInternal(string menuFileName)
        {
            if (!menuFileName.EndsWith(".menu", StringComparison.Ordinal))
            {
                menuFileName += ".menu";
            }

            byte[] buffer;
            try
            {
                buffer = ReadAFileBase(menuFileName);
            }
            catch (Exception ex)
            {
                PluginUtils.LogError("Could not read menu file '" + menuFileName + "' because " + ex.Message);
                return null;
            }
            try
            {
                var menu = new MenuInfo();
                using (var reader = new BinaryReader(new MemoryStream(buffer), Encoding.UTF8))
                {
                    if (!(reader.ReadString() == "CM3D2_MENU"))
                    {
                        return null;
                    }
                    reader.ReadInt32();
                    reader.ReadString();
                    menu.name = reader.ReadString();
                    reader.ReadString();
                    reader.ReadString();
                    reader.ReadInt32();
                    for (;;)
                    {
                        byte b = reader.ReadByte();
                        string text = string.Empty;
                        if (b == 0)
                        {
                            break;
                        }
                        for (int i = 0; i < (int)b; i++)
                        {
                            text = text + "\"" + reader.ReadString() + "\"";
                        }
                        if (!string.IsNullOrEmpty(text))
                        {
                            string stringCom = UTY.GetStringCom(text);
                            string[] stringList = UTY.GetStringList(text);
                            if (stringCom == "end")
                            {
                                break;
                            }
                            if (stringCom == "additem")
                            {
                                menu.modelFileName = stringList[1];
                                break;
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(menu.modelFileName))
                {
                    PluginUtils.LogWarning("Could not find model file in menu file '" + menuFileName + "'");
                    return null;
                }

                return menu;
            }
            catch (Exception ex2)
            {
                PluginUtils.LogWarning("Could not parse menu file '" + menuFileName + "' because " + ex2.Message);
                return null;
            }
        }

        private static byte[] ReadAFileBase(string filename)
        {
            byte[] result;
            using (AFileBase afileBase = GameUty.FileOpen(filename, null))
            {
                if (!afileBase.IsValid() || afileBase.GetSize() == 0)
                {
                    PluginUtils.LogError("AFileBase '" + filename + "' is invalid");
                    result = null;
                }
                else
                {
                    if (fileBuffer == null)
                    {
                        fileBuffer = new byte[Math.Max(500000L, afileBase.GetSize())];
                    }
                    else if ((long)fileBuffer.Length < afileBase.GetSize())
                    {
                        fileBuffer = new byte[afileBase.GetSize()];
                    }
                    afileBase.Read(ref fileBuffer, afileBase.GetSize());
                    result = fileBuffer;
                }
            }
            return result;
        }
    }
}