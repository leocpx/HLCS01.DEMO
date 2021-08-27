using System;

namespace HLCS01.SDK
{
    public class ExposedControllerPropertyAttribute : Attribute
    {
        public string description;
        public ExposedControllerPropertyAttribute(string _description)
        {
            description = _description;
        }
    }
}
