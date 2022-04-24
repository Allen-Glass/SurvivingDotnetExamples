namespace ReflectionExamples.Part3.Attributes
{
    public class Configuration : Attribute
    {
        public Configuration(string sectionName)
        {
            SectionName = sectionName;
        }

        public string SectionName { get; set; }
    }
}
