namespace CommentsDownloader.DTO.Interfaces
{
    public interface ICommentsRequest
    {
        string RequestUrl { get; set;}
        string Email { get; set; }
    }
}