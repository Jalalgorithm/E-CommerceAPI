namespace Backend.ApiModel.Base
{
    public class Pagination
    {
        public object Data { get; set; }
        public int? Page { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
}
