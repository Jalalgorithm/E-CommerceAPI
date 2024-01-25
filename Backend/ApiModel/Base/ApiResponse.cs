namespace Backend.ApiModel.Base
{
    public class ApiResponse
    {
        public string ErrorMessage { get; set; }
        public bool Successful => ErrorMessage == null;
        public object Result { get; set; }

    }
}
