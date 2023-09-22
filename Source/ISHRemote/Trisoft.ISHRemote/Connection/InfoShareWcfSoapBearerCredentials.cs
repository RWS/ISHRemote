/*
* Copyright (c) 2014 All Rights Reserved by the SDL Group.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*     http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

#if NET6_0_OR_GREATER
using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using SecurityKeyIdentifierClause = System.IdentityModel.Tokens.SecurityKeyIdentifierClause;
using SecurityToken = System.IdentityModel.Tokens.SecurityToken;


namespace Trisoft.ISHRemote.Connection
{
    /// <summary>
    /// SOAP web services (/ISHWS/OWCF/) with OpenIdConnect authentication need a way to pass the Access/Bearer token. This class wraps the token up in a SAML token which passes nicely over Windows Communication Foundation. Used in InfoShareWcfSoapWithOpenIdConnectConnection class
    /// </summary>
    internal sealed class BearerCredentials : ClientCredentials
    {
        internal readonly string AccessToken;

        internal BearerCredentials(string accessToken)
            => AccessToken = accessToken;

        private BearerCredentials(BearerCredentials cred) : base(cred)
            => AccessToken = cred.AccessToken;

        public override SecurityTokenManager CreateSecurityTokenManager()
            => new BearerCredentialsSecurityTokenManager(this);

        protected override ClientCredentials CloneCore()
            => new BearerCredentials(this);
    }

    internal sealed class BearerCredentialsSecurityTokenManager : ClientCredentialsSecurityTokenManager
    {
        internal BearerCredentialsSecurityTokenManager(BearerCredentials clientCredentials) : base(clientCredentials)
        {
        }

        public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
            => new BearerCredentialsSecurityTokenProvider((BearerCredentials)ClientCredentials);

        public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version)
            => new BearerCredentialsTokenSerializer();
    }

    internal sealed class BearerCredentialsSecurityTokenProvider : SecurityTokenProvider
    {
        private static readonly X509Certificate2 _certficate = CreateX509Certificate2();
        private readonly BearerCredentials _tokenCredentials;

        internal BearerCredentialsSecurityTokenProvider(BearerCredentials tokenCredentials)
        {
            _tokenCredentials = tokenCredentials;
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return WrapJwt(_tokenCredentials.AccessToken);
        }

        private static X509Certificate2 CreateX509Certificate2(string certName = "Client Tools")
        {
            var ecdsa = ECDsa.Create();
            var rsa = RSA.Create();
            var req = new CertificateRequest($"cn={certName}", rsa, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

            var password = Guid.NewGuid().ToString();
            return new X509Certificate2(cert.Export(X509ContentType.Pfx, password), password);
        }

        private static GenericXmlSecurityToken WrapJwt(string jwt)
        {
            // https://leastprivilege.com/2015/07/02/give-your-wcf-security-architecture-a-makeover-with-identityserver3/
            // https://github.com/IdentityServer/IdentityServer3/issues/1107
            // https://stackoverflow.com/questions/16312907/delivering-a-jwt-securitytoken-to-a-wcf-client
            // https://github.com/IdentityServer/IdentityServer3.Samples/tree/dev/source/Clients/WcfService

            var subject = new ClaimsIdentity("saml");
            subject.AddClaim(new Claim("jwt", jwt));

            // BW: Sign using the appropriate algorithm
            var securityKey = new X509SecurityKey(_certficate);
            const string algorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            const string digest = "http://www.w3.org/2001/04/xmlenc#sha256";
            var signingCredentials = new SigningCredentials(securityKey, algorithm, digest);

            var descriptor = new SecurityTokenDescriptor
            {
                TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0",
                Issuer = "urn:wrappedjwt", // BW: Changed TokenIssuerName to Issuer
                Subject = subject,
                SigningCredentials = signingCredentials
            };

            var handler = new Saml2SecurityTokenHandler
            {
                // BW: Do not include the token conditions
                SetDefaultTimesOnTokenCreation = false
            };
            var token = handler.CreateToken(descriptor) as Saml2SecurityToken;

            var sb = new StringBuilder();
            using var xmlWriter = XmlWriter.Create(sb);
            handler.WriteToken(xmlWriter, token);

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(sb.ToString());
            var xmlToken = new GenericXmlSecurityToken(
                xmlDocument.DocumentElement,
                null,
                DateTime.Now,
                DateTime.Now.AddHours(1),
                null,
                null,
                null);

            return xmlToken;
        }
    }

    internal sealed class BearerCredentialsTokenSerializer : SecurityTokenSerializer
    {
        protected override bool CanReadKeyIdentifierClauseCore(XmlReader reader) => throw new NotImplementedException();

        protected override bool CanReadKeyIdentifierCore(XmlReader reader) => throw new NotImplementedException();

        protected override bool CanReadTokenCore(XmlReader reader) => throw new NotImplementedException();

        protected override bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause) => throw new NotImplementedException();

        protected override bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier) => throw new NotImplementedException();

        protected override bool CanWriteTokenCore(SecurityToken token) => throw new NotImplementedException();

        protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader) => throw new NotImplementedException();

        protected override SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader) => throw new NotImplementedException();

        protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver) => throw new NotImplementedException();

        protected override void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause) => throw new NotImplementedException();

        protected override void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier) => throw new NotImplementedException();

        protected override void WriteTokenCore(XmlWriter writer, SecurityToken token)
        {
            var xmlToken = (GenericXmlSecurityToken)token;
            xmlToken.TokenXml.WriteTo(writer);
        }
    }
}
#endif
