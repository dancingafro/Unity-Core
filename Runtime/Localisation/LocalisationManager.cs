using CoreScript.Singleton;

namespace CoreScript.Localisation
{
    public class LocalisationManager : Singleton<LocalisationManager>
    {
        LocalisationData localisationData = null;
        public LocalisationData LocalisationData
        {
            get
            {
                if (localisationData == null)
                    localisationData = LocalisationData.Load();

                return localisationData;
            }
        }

        public string GetLocalisedValue(string key)
        {
            return LocalisationData.GetLocalisedValue(key);
        }
    }
}