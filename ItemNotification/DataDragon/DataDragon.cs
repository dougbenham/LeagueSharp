using System.Drawing;
using System.IO;
using System.Net;

namespace ItemNotification
{
    static class DataDragon
    {
        private static string _dataDragonItemsVersion;
        private static string DataDragonItemsVersion
        {
            get
            {
                if (_dataDragonItemsVersion == null)
                {
                    try
                    {
                        string result;
                        var request = WebRequest.CreateHttp("https://ddragon.leagueoflegends.com/realms/na.json");
                        using (var response = request.GetResponse())
                        {
                            using (var stream = new StreamReader(response.GetResponseStream()))
                            {
                                result = stream.ReadToEnd();
                            }
                        }
                        _dataDragonItemsVersion = JSON.Deserialize(result)["n"]["items"];
                    }
                    catch
                    {
                        _dataDragonItemsVersion = "5.11.1";
                    }
                }
                return _dataDragonItemsVersion;
            }
        }

        public static Bitmap GetItemBitmap(int id)
        {
            try
            {
                var request = WebRequest.CreateHttp("http://ddragon.leagueoflegends.com/cdn/" + DataDragonItemsVersion + "/img/item/" + id + ".png");
                using (var response = request.GetResponse())
                {
                    return new Bitmap(response.GetResponseStream());
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
