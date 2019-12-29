using DataAccessLayer;
using DHFL_Insta.fundsTransferByCustomerService;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;

namespace DHFL_Insta.Controllers
{
    public class TransferController : ApiController
    {
        Common c = new Common();
        string message = "";
        [HttpPost]
        public HttpResponseMessage TransferBal(string beneficiaryAccountNo, string beneficiaryIFSC, string beneficiaryMMID, string beneficiaryMobileNo, string Name, string address1,
                string emailID, string mobileNo, string uniqueRequestNo, string appID, string customerID, string debitAccountNo, float transferAmount)
        {

            DataSet ds = new DataSet();
            string URL = "Transfer/TransferBal&beneficiaryAccountNo=" + beneficiaryAccountNo + "&beneficiaryIFSC=" + beneficiaryIFSC + "&beneficiaryMMID=" + beneficiaryMMID + "&beneficiaryMobileNo=" + beneficiaryMobileNo + "&Name=" + Name + "&address1=" + address1 + "&emailID=" + emailID + "&mobileNo=" + mobileNo + "&uniqueRequestNo=" + uniqueRequestNo + "&appID=" + appID + "&customerID=" + customerID + "&debitAccountNo=" + debitAccountNo + "&transferAmount=" + transferAmount + "";
            ds = c.getInserlogrequest(URL);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //  APIBanking.Environment env = new APIBanking.Environments.YBL.UAT("testclient", "test@123", " 24255a45-9569-4906-9ef2-5d7b3e7a140a", "V8uI0gW4cD0gS3tO5wN6tA1dH2oP3uY2bS7kC7oS4pI0nF0hO2", null);
            APIBanking.Environment env = new APIBanking.Environments.YBL.UAT("5280521", "0inad", "adb08bda-05a2-42c7-acaf-1a63a4c883f4", "gE8oG1lF0aM5iJ5lX8iE1fQ2iF1xG3qV5bI6hJ1oR0kN6kC0fU", null);
            transfer gTransfer = new transfer();
            transferRequest gTransferRequest = new transferRequest();
            transferResponse gTransferResponse = new transferResponse();

            beneficiaryDetailType b = new beneficiaryDetailType();
            b.beneficiaryAccountNo = beneficiaryAccountNo;
            b.beneficiaryIFSC = beneficiaryIFSC;
            b.beneficiaryMMID = beneficiaryMMID;
            b.beneficiaryMobileNo = beneficiaryMobileNo;

            beneficiaryType bt = new beneficiaryType();
            nameType nm = new nameType();
            nm.Item = Name;

            AddressType ad = new AddressType();
            ad.address1 = address1;
            //ad.address2 = "";B
            // ad.address3 = "";
            // ad.city = "";
            ad.country = "IN";
            //ad.postalCode = "";

            contactType ct = new contactType();
            ct.emailID = emailID;
            ct.mobileNo = mobileNo;

            b.beneficiaryName = nm;
            b.beneficiaryAddress = ad;
            b.beneficiaryContact = ct;



            gTransfer.beneficiary = bt;
            gTransfer.beneficiary.Item = b;
            gTransfer.version = "2";
            gTransfer.uniqueRequestNo = uniqueRequestNo; //Ihno
            gTransfer.appID = appID;
            gTransfer.customerID = customerID;
            gTransfer.debitAccountNo = debitAccountNo;
            gTransfer.transferAmount = transferAmount;
            gTransfer.transferType = transferTypeType.IMPS;
            gTransfer.transferCurrencyCode = currencyCodeType.INR;
            gTransfer.remitterToBeneficiaryInfo = "FUND TRANSFER";
            try
            {

                gTransferResponse = APIBanking.DomesticRemittanceClient.getTransfer(env, gTransfer);
                //return Request.CreateResponse(HttpStatusCode.OK, getBalanceResponse);
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), gTransferResponse.ToString());
                c.InsertResponse(gTransferResponse.transactionStatus.subStatusCode, gTransferResponse.transactionStatus.statusCode.ToString(), gTransferResponse.requestReferenceNo, gTransferResponse.transactionStatus.bankReferenceNo);
                return this.Request.CreateResponse(HttpStatusCode.OK, gTransferResponse);
            }
            catch (FaultException ex)
            {
                String faultCode = ex.Code.SubCode.Name;
                String FaultReason = ex.Message;
                message = faultCode + " - " + FaultReason;
                HttpError myCustomError = new HttpError();
                myCustomError.Add("ErrorCode", faultCode);
                myCustomError.Add("Errormsg", FaultReason);
                myCustomError.Add("Ihno", uniqueRequestNo);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(myCustomError.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, myCustomError);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes);
                c.InsertResponse(faultCode, FaultReason, uniqueRequestNo, "");
                c.writelog(ex.Message, "FaultException", DateTime.Now, "", "");
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, myCustomError);
            }

            catch (Exception ex)
            {
                HttpError myCustomError = new HttpError();
                myCustomError.Add("ErrorCode", 500);
                myCustomError.Add("Errormsg", "InternerlServer Error");
                myCustomError.Add("Ihno", uniqueRequestNo);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(myCustomError.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, myCustomError);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes);
                c.InsertResponse("500", ex.Message, uniqueRequestNo, "");
                c.writelog(ex.Message, "TransferBal", DateTime.Now, "", "");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, myCustomError);
            }
        }
    }
}
