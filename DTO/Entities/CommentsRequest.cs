using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommentsDownloader.DTO.Interfaces;

namespace CommentsDownloader.DTO.Entities
{
    public class CommentsRequest : Entity, ICommentsRequest
    {
        public string RequestUrl { get; set; }
        public string Email { get; set; }
        public string TempFileDirectory { get; set; }
        public bool CommentsFetched { get; set; }
        public bool MailSent { get; set; }
    }
}
