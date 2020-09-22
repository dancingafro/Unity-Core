namespace CoreScript.Localisation
{
    [System.Serializable]
    public struct LocalisedString
    {
        public string key;

        public LocalisedString(string key)
        {
            this.key = key;
        }

        public string Value { get { return LocalisationManager.Instance.GetLocalisedValue(key); } }

        public static implicit operator LocalisedString(string key)
        {
            return new LocalisedString(key);
        }
    }
}