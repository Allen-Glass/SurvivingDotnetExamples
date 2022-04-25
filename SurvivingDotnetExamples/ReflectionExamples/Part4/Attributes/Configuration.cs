namespace ReflectionExamples.Part4.Attributes
{
    public class Configuration : Attribute
    {
        public Configuration(string sectionName = "")
        {
            if (!string.IsNullOrEmpty(sectionName))
                SectionName = sectionName;
        }

        public string SectionName { get; set; }

        public void SetSectionName(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
                return;

            SectionName = sectionName.Replace("Settings", "");
        }
    }
}
