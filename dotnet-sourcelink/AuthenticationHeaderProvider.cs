using System;
using System.Net.Http.Headers;
using System.Text;

namespace SourceLink
{
    public interface IAuthenticationHeaderValueProvider
    {
        AuthenticationHeaderValue GetValue();
    }

    internal class BasicAuthenticationHeaderValueProvider : IAuthenticationHeaderValueProvider
    {
        private readonly string _username;
        private readonly string _password;
        private readonly Encoding _encoding;

        public BasicAuthenticationHeaderValueProvider(string username, string password, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username),"Invalid username value");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password),"Invalid password value");

            _username = username;
            _password = password;
            _encoding = encoding ?? Encoding.ASCII;
        }

        public AuthenticationHeaderValue GetValue()
        {
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(_encoding.GetBytes($"{_username}:{_password}")));
        }
    }
}
