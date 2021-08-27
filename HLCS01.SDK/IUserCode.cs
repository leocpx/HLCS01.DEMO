namespace SDK
{
    public interface IUserCode
    {
        T GetExecute<T>(string Method, params object[] _objects);
        void Execute(string Method, params object[] _objects);
        string ControllerModuleName { get; set; }
    }
}
