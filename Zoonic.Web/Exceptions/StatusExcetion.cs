
namespace Zoonic.Web.Exceptions
{
    public class StatusException : BaseException
    {
        public StatusException()
        {

        }
        public StatusException(int status):base(string.Empty,status)
        {
            
        }
        public StatusException(string message, int status = 500) : base(message)
        {

        }
        public StatusException(string message, System.Exception exception) : base(message, exception)
        {

        }
    }
}
