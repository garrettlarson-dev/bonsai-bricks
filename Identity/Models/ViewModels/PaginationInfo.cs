namespace Identity.Models.ViewModels;

public class PaginationInfo
{
    public int TotalItems { get; set; }
    
    public int ItemsPerPage { get; set; }
    
    public int CurrentPage { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalItems / ItemsPerPage);

}