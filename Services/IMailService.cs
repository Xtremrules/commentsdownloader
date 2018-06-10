
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using CommentsDownloader.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommentsDownloader.Services
{
    public interface IMailService
    {
        void SendMail(Message message);
    }
}
