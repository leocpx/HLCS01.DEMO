using System;

namespace HLCS01.SDK
{
    public class ExposedControllerMethodAttribute : Attribute
    {
        public string _description;
        public ExposedControllerMethodAttribute(string description)
        {
            _description = description;
        }
    }
}
