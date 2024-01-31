using Microsoft.EntityFrameworkCore;

namespace Backend.Model
{
    [Keyless]
    public class OtherImagesForProduct
    {
        public List<string> ImagesForProduct { get; set; } = new List<string>();
    }
}
