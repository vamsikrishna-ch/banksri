﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Configuration;
using System.Net;
using System.Net.Security;

namespace APIBanking
{
    public interface Environment
    {
        Boolean needsHTTPBasicAuth();
        String getUser();
        String getPassword();
        String getClientId();
        String getClientSecret();
        EndpointAddress getEndpointAddress(String serviceName);
        Hashtable getHeaders();
        System.Net.SecurityProtocolType getSecurityProtocol();
        Uri getProxyAddress();
        X509Certificate2 getClientCertificate();
        Boolean needsClientCertificate();

    }
    namespace Environments.YBL
    {
        public class UAT : Environment
        {
            private String user;
            private String password;
            private String client_id;
            private String client_secret;
            private Uri proxyAddress;
            private String pkcs12FilePath;
            private String pkcs12Password;

            //private String pkcs12FilePath="G:\\Yes_bank_documents\\cer\\p12Karvy.p12";
            //private String pkcs12Password="123";

            public UAT(String user, String password, String client_id, String client_secret, String pkcs12FilePath = null, String pkcs12Password = null, Uri proxyAddress = null)
            {

                this.user = user;
                this.password = password;
                this.client_id = client_id;
                this.client_secret = client_secret;
                this.proxyAddress = proxyAddress;
                //this.pkcs12FilePath = pkcs12FilePath;
                //this.pkcs12Password = pkcs12Password;
                this.pkcs12FilePath = ConfigurationManager.AppSettings["CertificatePath"].ToString();
                this.pkcs12Password = ConfigurationManager.AppSettings["CertificatePwd"].ToString();
            }
            public UAT(String user, String password, String client_id, String client_secret, Uri proxyAddress = null)
                : this(user, password, client_id, client_secret, null, null, proxyAddress)
            {
            }

            public Uri getProxyAddress()
            {
                return this.proxyAddress;
            }
            public Boolean needsHTTPBasicAuth()
            {
                return true;
            }
            public String getUser()
            {
                return this.user;
            }
            public String getPassword()
            {
                return this.password;
            }
            public String getClientId()
            {
                return this.client_id;
            }
            public String getClientSecret()
            {
                return this.client_secret;
            }
            public Boolean needsClientCertificate()
            {
                return (this.pkcs12FilePath != null) ? true : false;
            }
            public EndpointAddress getEndpointAddress(String serviceName)
            {
                String baseURL = "https://sky.yesbank.in";
                if (needsClientCertificate())
                {
                    baseURL += ":444";
                }
                if (serviceName == "fundsTransferByCustomerService")
                {
                    return new EndpointAddress(baseURL + "/app/live/fundsTransferByCustomerServiceHttpService");
                }
                else
                    if (serviceName == "fundsTransferByCustomerService2")
                    {
                        return new EndpointAddress(baseURL + "/app/live/fundsTransferByCustomerService2");
                    }
                    else
                        if (serviceName == "InwardRemittanceByPartnerService")
                        {
                            return new EndpointAddress(baseURL + "/app/live/InwardRemittanceByPartnerServiceHttpService");
                        }
                        else
                            if (serviceName == "DomesticRemittanceByPartnerService")
                            {
                                return new EndpointAddress(baseURL + "/app/live/DomesticRemittanceService");
                            }
                            else
                            {
                                return new EndpointAddress(baseURL + "/app/live/ssl/" + serviceName);
                            }
            }

            public Hashtable getHeaders()
            {
                Hashtable headers = new Hashtable();
                headers.Add("X-IBM-Client-Id", client_id);
                headers.Add("X-IBM-Client-Secret", client_secret);
                if (needsClientCertificate())
                {
                    headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + password)));

                }
                return headers;
            }
            public System.Net.SecurityProtocolType getSecurityProtocol()
            {
                return System.Net.SecurityProtocolType.Tls;
            }
            public X509Certificate2 getClientCertificate()
            {

                return new X509Certificate2(this.pkcs12FilePath, this.pkcs12Password);
            }
            //      private static bool RemoteCertificateValidate(

            //object sender, X509Certificate cert,

            // X509Chain chain, SslPolicyErrors error)
            //      {

            //          // trust any certificate!!!

            //          System.Console.WriteLine("Warning, trust any certificate");

            //          return true;

            //      }
        }
    }
}
