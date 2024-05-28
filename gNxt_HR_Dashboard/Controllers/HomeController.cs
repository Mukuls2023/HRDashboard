using gNxt_HR_Dashboard.Helper;
using gNxt_HR_Dashboard.Models;
using gNxt_HR_Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace gNxt_HR_Dashboard.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            slideListData lstbir = new slideListData();
            lstbir.lstCelebration = GetCelebrationData();
            return View("Index", "_LayoutPage", lstbir);
        }
        public ActionResult LogIn()
        {
            return View("LogIn", "_LayoutPageLogin");
        }

        public ActionResult MainInformation(string Month, int? requestedLeave)
        {
            int FilterDate = Convert.ToInt32(WebConfigurationManager.AppSettings.Get("FilterDate"));
            ViewBag.UserName = Convert.ToString(Session["UserName"]);
            MainInformationModels objMainInformation = new MainInformationModels();
            objMainInformation.lstMissedPunch = GetMissedPunchData();
            objMainInformation.lstNoticeData = GetNoticeBoardData();
            objMainInformation.LatestNoticeText = objMainInformation.lstNoticeData[0].NoticeText;
            objMainInformation.PastNoticeText = objMainInformation.lstNoticeData[1].NoticeText;
            objMainInformation.PaySlipHeader = GetSalaryHeaderDetails(Month, FilterDate);
            objMainInformation.PaySlipDesc = GetSalaryDescDetails(Month, FilterDate);
            objMainInformation.PaySlipDesc.GrossSalaryRate = objMainInformation.PaySlipHeader.BasicSalaryRate + objMainInformation.PaySlipDesc.HRARate + objMainInformation.PaySlipDesc.SpecialAllowRate;
            objMainInformation.PaySlipDesc.GrossSalaryPayable = objMainInformation.PaySlipHeader.BasicSalaryPayable + objMainInformation.PaySlipDesc.HRAPayable + objMainInformation.PaySlipDesc.SpecialAllowPayable;
            objMainInformation.PaySlipDesc.NetPay = objMainInformation.PaySlipDesc.GrossSalaryRate - objMainInformation.PaySlipDesc.DeductionTotal;
            objMainInformation.PaySlipDesc.NetPayInWords = NumberToWordsConverter.Convert(objMainInformation.PaySlipDesc.NetPay);
            if (Month.Length > 0)
            {
                TempData["showpayslip"] = "true";
                TempData["showApplyleave"] = "false2";
            }
            else
            {
                TempData["showpayslip"] = "false";
                TempData["showApplyleave"] = "false";
                if (requestedLeave > 0)
                {
                    TempData["showApplyleave"] = "true";
                }
            }
            objMainInformation.lstLeaveBalance = GetLeaveBalance();
            objMainInformation.CasualLeaveTotal = ViewBag.TotalCasualValue;
            objMainInformation.EarnLeaveTotal = Convert.ToDecimal(ViewBag.TotalEarnValue);
            objMainInformation.SickLeaveTotal = ViewBag.TotalSickValue;
            var abcd = ConvertStringToHex("1234");
            return View("MainInformation", "_LayoutPageMainInformation", objMainInformation);
        }

        [HttpPost]
        public ActionResult LogIn(LoginModels objlogin)
        {
            ViewBag.Text = "";
            string dbuser = string.Empty;
            string dbpwd = string.Empty;
            string username = objlogin.Username;
            string pwd = objlogin.Userpassword;
            string strConn = WebConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            OleDbConnection Conn = new OleDbConnection(strConn);
            OleDbDataReader rdr;
            string sqlUserName;
            sqlUserName = "SELECT A.USER_ID, A.NEWPASSWORD,A.EMP_NAME,B.COMPANY_CODE FROM ath_password_mst A,PAY_EMPLOYEE_MST B WHERE A.USER_ID=B.EMP_CODE AND A.USER_ID ='" + username + "' and A.NEWPASSWORD='" + pwd + "'";
            OleDbCommand com = new OleDbCommand(sqlUserName, Conn);
            Conn.Open();
            rdr = com.ExecuteReader();

            while (rdr.Read())
            {
                Session["USER_ID"] = rdr[0].ToString();
                dbuser = rdr[0].ToString();
                dbpwd = rdr[1].ToString();
                ViewBag.UserName = rdr[2].ToString();
                Session["UserName"] = rdr[2].ToString();
                Session["COMPANY_CODE"] = rdr[3].ToString();
            }
            if (username == dbuser && pwd == dbpwd)
            {
                return MainInformation("", 0);
            }
            else
            {
                ViewBag.Text = "Wrong Details";
                return View("LogIn", "_LayoutPageLogin");
            }

            return View("");
        }

        public List<CelebrationModels> GetCelebrationData()
        {
            List<CelebrationModels> lst1 = new List<CelebrationModels>();
            string strConn = WebConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            OleDbConnection Conn = new OleDbConnection(strConn);
            OleDbDataReader rdr;
            string sqlUserName;
            string DOB;
            string DOJ;
            string CompanyCode;
            string YearWithSuffix;
            string TodaysDate = DateTime.Now.ToString("dd-MM");
            sqlUserName = "select EMP_CODE, EMP_NAME,CASE WHEN TO_CHAR(EMP_DOB,'DD-MM') = '" + TodaysDate + "'  THEN 'DOB' WHEN TO_CHAR(EMP_DOJ,'DD-MM') = '" + TodaysDate + "'   THEN 'DOJ' END AS Type,EMP_DOB,EMP_DOJ,COMPANY_CODE from pay_employee_mst where TO_CHAR(EMP_DOB,'DD-MM') = '" + TodaysDate + "' OR TO_CHAR(EMP_DOJ,'DD-MM') = '" + TodaysDate + "'";
            OleDbCommand com = new OleDbCommand(sqlUserName, Conn);
            Conn.Open();
            rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                CelebrationModels obj1 = new CelebrationModels();
                obj1.EmpCode = rdr[0].ToString();
                obj1.EmpName = rdr[1].ToString();
                obj1.CelebrationType = rdr[2].ToString();
                DOB = Convert.ToDateTime(rdr[3]).ToString("MMM dd");
                DOJ = Convert.ToDateTime(rdr[4]).ToString("MMM dd");
                CompanyCode = rdr[5].ToString();
                int TotalYear = new DateTime((DateTime.Now - Convert.ToDateTime(rdr[4])).Ticks).Year;
                YearWithSuffix = TotalYear + Getsuffix(TotalYear);
                string Imageurl = "../Image/UserImages/" + "0"+ CompanyCode+ "_" + obj1.EmpCode + ".jpg";
                obj1.Image = Imageurl;
                //obj1.Image = "../Image/user.jpg";
                if (rdr[2].ToString() == "DOB")
                {
                    obj1.MessageText = DOB + " - " + "Happy Birthday!";
                }
                if (rdr[2].ToString() == "DOJ")
                {
                    obj1.MessageText = DOJ + " - " + YearWithSuffix + " Anniversary";
                }

                lst1.Add(obj1);
            }
            return lst1;
        }

        public string Getsuffix(int number)
        {
            string result = string.Empty;
            if (number > 3 && number < 21) return result = "th";
            switch (number % 10)
            {
                case 1:
                    return result = "st";
                case 2:
                    return result = "nd";
                case 3:
                    return result = "rd";
                default:
                    return result = "th";
            }
        }

        public List<MissedPunchModels> GetMissedPunchData()
        {
            List<MissedPunchModels> lst1 = new List<MissedPunchModels>();
            string strConn = WebConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            OleDbConnection Conn = new OleDbConnection(strConn);
            OleDbDataReader rdr;
            int counter = 1;
            ViewBag.MissedPunchText = "No Missed Punch Data Available!";
            string sqlUserName;
            string MonthName;
            int CurrentDate = Convert.ToInt32(DateTime.Now.AddMonths(0).ToString("dd"));
            if (CurrentDate < 16)
            {
                MonthName = DateTime.Now.AddMonths(-1).ToString("MMMM-yyyy").ToUpper();
            }
            else
            {
                MonthName = DateTime.Now.AddMonths(0).ToString("MMMM-yyyy").ToUpper();
            }
            string datefilter = MonthName;
            sqlUserName = "select ATTN_DATE,to_char(IN_TIME,'DD-MON-YYYY HH24:MI:SS') AS IN_TIME,to_char(OUT_TIME,'DD-MON-YYYY HH24:MI:SS')AS OUT_TIME from PAY_DALIY_ATTN_TRN where Company_Code = '" + Session["COMPANY_CODE"] + "' AND EMP_CODE= '" + Session["USER_ID"] + "' AND TO_CHAR(ATTN_DATE,'MON-YYYY') = '" + datefilter + "' AND to_char(IN_TIME,'HH24:Mi')<>'00:00' AND to_char(OUT_TIME,'HH24:Mi')='00:00'";
            OleDbCommand com = new OleDbCommand(sqlUserName, Conn);
            Conn.Open();
            rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                ViewBag.MissedPunchText = "";
                MissedPunchModels obj1 = new MissedPunchModels();
                obj1.SNO = counter;
                obj1.InTime = rdr[1].ToString();
                obj1.OutTime = rdr[2].ToString();
                lst1.Add(obj1);
                counter++;
            }
            return lst1;
        }

        public List<NoticeModels> GetNoticeBoardData()
        {
            List<NoticeModels> lst1 = new List<NoticeModels>();
            string strConn = WebConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            OleDbConnection Conn = new OleDbConnection(strConn);
            OleDbDataReader rdr;
            string sqlUserName;
            sqlUserName = "select NOTICE_TEXT from PAY_NOTICE_ATTN_MST WHERE Company_Code = '" + Session["COMPANY_CODE"] + "' ORDER BY NOTICE_DATE DESC fetch first 2 rows only";
            OleDbCommand com = new OleDbCommand(sqlUserName, Conn);
            Conn.Open();
            rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                NoticeModels obj1 = new NoticeModels();
                obj1.NoticeText = rdr[0].ToString();
                lst1.Add(obj1);
            }
            return lst1;
        }

        [HttpPost]
        public ActionResult MainInformation(MainInformationModels objMainInformation)
        {
            string empcode = Convert.ToString(Session["USER_ID"]);
            string companycode = Convert.ToString(Session["COMPANY_CODE"]);
            string LeaveType = objMainInformation.LeaveRequest.LeaveType;
            OleDbDataReader rdr;
            string HalfDay = "";
            string ToDate = "";
            string fromdate = "";
            string Approver = "";
            int LeaveDays = 0;
            string AUTO_KEY_REF = "";
            string APP_EMP_CODE = "";
            string Ref_Date = DateTime.Now.ToString("dd-MMM-yy");
            TempData["showApplyleave"] = "true";
            TempData["showpayslip"] = "false";
            ViewBag.ApplyLeaveText = "";
            if (LeaveType == "FullLeave")
            {
                HalfDay = "N";
                fromdate = objMainInformation.LeaveRequest.FromDate.ToString("dd-MMM-yy");
                ToDate = objMainInformation.LeaveRequest.ToDate.ToString("dd-MMM-yy");
                if (Convert.ToDateTime(fromdate) < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yy")) || Convert.ToDateTime(ToDate) < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yy")) || Convert.ToDateTime(ToDate) < Convert.ToDateTime(fromdate))
                {
                    ViewBag.ApplyLeaveText = "Please select Correct Dates";
                    return MainInformation("", 1);
                }
                LeaveDays = Convert.ToInt32((Convert.ToDateTime(ToDate) - Convert.ToDateTime(fromdate)).TotalDays);
            }
            if (LeaveType == "HalfLeave")
            {
                HalfDay = "Y";
                ToDate = objMainInformation.LeaveRequest.HalfDayDate.ToString("dd-MMM-yy");
                fromdate = DateTime.Now.ToString("dd-MMM-yy");
                if (Convert.ToDateTime(ToDate) < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yy")))
                {
                    ViewBag.ApplyLeaveText = "Please select Correct Dates";
                    return MainInformation("", 1);
                }
                LeaveDays = 0;
                string leaveTime = objMainInformation.LeaveRequest.leaveTime;
            }
            string LeaveReasion = objMainInformation.LeaveRequest.LeaveReasion;

            string strConn = WebConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            OleDbConnection Conn = new OleDbConnection(strConn);

            string sqlUserName = "select EMP_HOD_CODE from pay_employee_mst where emp_code = '" + Session["USER_ID"] + "'";
            OleDbCommand com = new OleDbCommand(sqlUserName, Conn);
            Conn.Open();
            rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                Approver = rdr[0].ToString();
            }
            Conn.Close();
            AUTO_KEY_REF = AutoKeyRefGenerater();
            APP_EMP_CODE = GetAppEmpCode();

            string sqlUserName2 = "INSERT INTO PAY_LEAVE_APP_TRN(AUTO_KEY_REF,REF_DATE,COMPANY_CODE,EMP_CODE,FROM_DATE,TO_DATE,LDAYS,REASON,APP_STATUS,HR_STATUS,ADDUSER,ADDDATE,HALF_DAY,REC_EMP_CODE,APP_EMP_CODE) VALUES(" + AUTO_KEY_REF + ", '" + Ref_Date + "', " + Session["COMPANY_CODE"] + ", '" + Session["USER_ID"] + "', '" + ToDate + "', '" + fromdate + "', " + LeaveDays + ",'" + LeaveReasion + "', 'O','O', 'HR', '" + ToDate + "','" + HalfDay + "','" + Approver + "','" + APP_EMP_CODE + "')";
            OleDbCommand com2 = new OleDbCommand(sqlUserName2, Conn);
            Conn.Open();
            int Result = com2.ExecuteNonQuery();
            if (Result > 0)
            {
                ViewBag.ApplyLeaveText = "Leave Applied successfully!";
                return MainInformation("", 1);
            }
            return MainInformation("", 0);
        }

        public string AutoKeyRefGenerater()
        {
            string Result = string.Empty;
            string strConn = WebConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            OleDbConnection Conn = new OleDbConnection(strConn);
            OleDbDataReader rdr;
            string sqlUserName;
            sqlUserName = "SELECT Max(TO_NUMBER(substr(AUTO_KEY_REF,1,length(AUTO_KEY_REF)-2))) AS AUTO_KEY FROM PAY_LEAVE_APP_TRN  WHERE Company_Code = '" + Session["COMPANY_CODE"] + "'";
            OleDbCommand com = new OleDbCommand(sqlUserName, Conn);
            Conn.Open();
            rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                Result = (Convert.ToInt32(rdr[0]) + 1).ToString();
            }
            Conn.Close();
            Result = Convert.ToInt32(Result) + 1 + "0" + Session["COMPANY_CODE"];
            return Result;
        }

        public string GetAppEmpCode()
        {
            string Result = string.Empty;
            string strConn = WebConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            OleDbConnection Conn = new OleDbConnection(strConn);
            OleDbDataReader rdr;
            string sqlUserName;
            sqlUserName = "SELECT EMP_CODE FROM PAY_EMPLOYEE_MST WHERE COMPANY_CODE='" + Session["COMPANY_CODE"] + "' AND IS_HR_HOD='Y' AND(EMP_LEAVE_DATE IS NULL OR EMP_LEAVE_DATE = '' OR EMP_LEAVE_DATE >= '" + DateTime.Now.ToString("dd-MMM-yyyy") + "')";
            OleDbCommand com = new OleDbCommand(sqlUserName, Conn);
            Conn.Open();
            rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                Result = rdr[0].ToString();
            }
            Conn.Close();
            return Result;
        }

        public PaySlipHeaderModels GetSalaryHeaderDetails(string Month, int FilterDate)
        {
            PaySlipHeaderModels objpayslipHeader = new PaySlipHeaderModels();
            int CurrentDate = Convert.ToInt32(DateTime.Now.AddMonths(0).ToString("dd"));
            if (CurrentDate < FilterDate)
            {
                objpayslipHeader.MontnName1 = DateTime.Now.AddMonths(-2).ToString("MMMM");
                objpayslipHeader.MontnNo1 = DateTime.Now.AddMonths(-2).ToString("dd-MMM-yyyy");
                objpayslipHeader.MontnName2 = DateTime.Now.AddMonths(-3).ToString("MMMM");
                objpayslipHeader.MontnNo2 = DateTime.Now.AddMonths(-3).ToString("dd-MMM-yyyy");
                objpayslipHeader.MontnName3 = DateTime.Now.AddMonths(-4).ToString("MMMM");
                objpayslipHeader.MontnNo3 = DateTime.Now.AddMonths(-4).ToString("dd-MMM-yyyy");
            }
            else
            {
                objpayslipHeader.MontnName1 = DateTime.Now.AddMonths(-1).ToString("MMMM");
                objpayslipHeader.MontnNo1 = DateTime.Now.AddMonths(-1).ToString("dd-MMM-yyyy");
                objpayslipHeader.MontnName2 = DateTime.Now.AddMonths(-2).ToString("MMMM");
                objpayslipHeader.MontnNo2 = DateTime.Now.AddMonths(-2).ToString("dd-MMM-yyyy");
                objpayslipHeader.MontnName3 = DateTime.Now.AddMonths(-3).ToString("MMMM");
                objpayslipHeader.MontnNo3 = DateTime.Now.AddMonths(-3).ToString("dd-MMM-yyyy");
            }
            objpayslipHeader.Year = DateTime.Now.AddMonths(0).ToString("yyyy");
            string strConn = WebConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            OleDbConnection Conn = new OleDbConnection(strConn);
            OleDbDataReader rdr;
            ViewBag.SalaryMessageText = "";
            string LastDateOfMonth = "";
            DateTime startDate;
            if (Month.Length > 0)
            {
                DateTime now = Convert.ToDateTime(Month);
                startDate = new DateTime(now.Year, now.Month, 1);
                DateTime firstDayOfNextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                LastDateOfMonth = firstDayOfNextMonth.AddDays(-1).ToString("dd-MMM-yyyy");
            }
            else
            {
                DateTime now = Convert.ToDateTime(objpayslipHeader.MontnNo1);
                startDate = new DateTime(now.Year, now.Month, 1);
                DateTime firstDayOfNextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                LastDateOfMonth = firstDayOfNextMonth.AddDays(-1).ToString("dd-MMM-yyyy");
            }
            int DaysInMonth = 0;
            if (Month.Length > 0)
            {
                int MonthNo = DateTime.ParseExact(Convert.ToDateTime(Month).ToString("MMMM"), "MMMM", CultureInfo.CurrentCulture).Month;
                DaysInMonth = System.DateTime.DaysInMonth(Convert.ToInt32(objpayslipHeader.Year), Convert.ToInt32(MonthNo));
            }
            else
            {
                DaysInMonth = System.DateTime.DaysInMonth(Convert.ToInt32(objpayslipHeader.Year), Convert.ToInt32(DateTime.Now.AddMonths(0).ToString("MM")));
            }
            objpayslipHeader.PaySlipMonths = Convert.ToDateTime(LastDateOfMonth).ToString("MMMM");
            if (Month.Length > 0)
            {
                string Givenyear = "";
                string GivenMonth = "";
                Givenyear = Convert.ToDateTime(Month).ToString("yyyy");
                GivenMonth = Convert.ToDateTime(Month).ToString("MM");
                LastDateOfMonth = GetLastDateOfMonth(Convert.ToInt32(Givenyear), Convert.ToInt32(GivenMonth));
                objpayslipHeader.PaySlipMonths = Convert.ToDateTime(LastDateOfMonth).ToString("MMMM");
            }
            string sqlUserName;
            sqlUserName = "select A.EMP_CODE,C.EMP_NAME,C.EMP_DOJ,A.DESG_DESC,A.DEPARTMENT,A.BANKACCTNO,A.WDAYS,SAL_DATE,BASICSALARY, PAYABLESALARY,C.EMP_FNAME from pay_sal_trn A, Pay_salaryhead_mst b, PAY_EMPLOYEE_MST C WHERE A.COMPANY_CODE=b.COMPANY_CODE AND ";
            sqlUserName += "A.SALHEADCODE=B.CODE AND A.COMPANY_CODE=c.COMPANY_CODE AND A.EMP_CODE=c.EMP_CODE AND a.EMP_CODE ='" + Session["USER_ID"] + "' AND SAL_DATE=TO_DATE('" + LastDateOfMonth + "','DD-MON-YYYY') AND ISARREAR='N' fetch first 1 rows only";
            OleDbCommand com = new OleDbCommand(sqlUserName, Conn);
            Conn.Open();
            rdr = com.ExecuteReader();
            if (rdr.Read())
            {
                ViewBag.SalaryMessageText = "";
                objpayslipHeader.EmpCode = rdr[0].ToString();
                objpayslipHeader.Name = rdr[1].ToString();
                objpayslipHeader.JoiningDate =Convert.ToDateTime(rdr[2]).ToString("dd-MMM-yyyy");
                objpayslipHeader.Designation = rdr[3].ToString();
                objpayslipHeader.Department = rdr[4].ToString();
                objpayslipHeader.BankAC = rdr[5].ToString();
                objpayslipHeader.Rate = DaysInMonth.ToString();
                objpayslipHeader.Payable = rdr[6].ToString();
                objpayslipHeader.SalaryDate = rdr[7].ToString();
                objpayslipHeader.BasicSalaryPayable = Convert.ToInt32(rdr[8]);
                objpayslipHeader.BasicSalaryRate = Convert.ToInt32(rdr[9]);
                objpayslipHeader.FatherHusbandName = rdr[10].ToString();
            }
            else
            {
                ViewBag.SalaryMessageText = "No Data Available!";           
            }
            Conn.Close();
            return objpayslipHeader;
        }

        public PaySlipDescModels GetSalaryDescDetails(string Month, int FilterDate)
        {
            PaySlipDescModels objpayDescInfo = new PaySlipDescModels();
            int CurrentDate = Convert.ToInt32(DateTime.Now.AddMonths(0).ToString("dd"));
            if (CurrentDate < FilterDate)
            {
                objpayDescInfo.CurrentDate = DateTime.Now.AddMonths(-1).ToString("dd-MMM-yyyy");
            }
            else
            {
                objpayDescInfo.CurrentDate = DateTime.Now.AddMonths(0).ToString("dd-MMM-yyyy");
            }

            string strConn = WebConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            OleDbConnection Conn = new OleDbConnection(strConn);
            string sqlUserName;
            string LastDateOfMonth = "";
            DateTime startDate;
            if (Month.Length > 0)
            {
                DateTime now = Convert.ToDateTime(Month);
                startDate = new DateTime(now.Year, now.Month, 1);
                DateTime firstDayOfNextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                LastDateOfMonth = firstDayOfNextMonth.AddDays(-1).ToString("dd-MMM-yyyy");
            }
            else
            {
                DateTime now = Convert.ToDateTime(objpayDescInfo.CurrentDate);
                startDate = new DateTime(now.Year, now.Month, 1);
                DateTime firstDayOfNextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                LastDateOfMonth = firstDayOfNextMonth.AddDays(-1).ToString("dd-MMM-yyyy");
            }
            if (Month.Length > 0)
            {
                string Givenyear = "";
                string GivenMonth = "";
                Givenyear = Convert.ToDateTime(Month).ToString("yyyy");
                GivenMonth = Convert.ToDateTime(Month).ToString("MM");
                LastDateOfMonth = GetLastDateOfMonth(Convert.ToInt32(Givenyear), Convert.ToInt32(GivenMonth));
            }
            sqlUserName = "SELECT b.NAME, PAYABLEAMOUNT, ACTUALAMOUNT from pay_sal_trn A, Pay_salaryhead_mst b, PAY_EMPLOYEE_MST C WHERE A.COMPANY_CODE=b.COMPANY_CODE AND ";
            sqlUserName += "TO_CHAR(A.SALHEADCODE)=TO_CHAR(B.CODE) AND A.COMPANY_CODE=c.COMPANY_CODE AND A.EMP_CODE=c.EMP_CODE AND a.EMP_CODE ='" + Session["USER_ID"] + "' AND SAL_DATE=TO_DATE('" + LastDateOfMonth + "','DD-MON-YYYY') AND ISARREAR='N'";

            Conn.Open();
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            adapter.SelectCommand = new OleDbCommand(sqlUserName, Conn);
            DataSet ds = new DataSet("SalaryDescDetails");
            adapter.Fill(ds);
            DataTable customerTable = ds.Tables[0];
            foreach (DataRow dr in customerTable.Rows)
            {
                if (dr[0].ToString() == "HRA")
                {
                    objpayDescInfo.HRARate = Convert.ToInt32(dr["PAYABLEAMOUNT"]);
                    objpayDescInfo.HRAPayable = Convert.ToInt32(dr["ACTUALAMOUNT"]);
                }
                if (dr[0].ToString() == "SPECIAL  ALL. / MISC.  ALLOW")
                {
                    objpayDescInfo.SpecialAllowRate = Convert.ToInt32(dr["PAYABLEAMOUNT"]);
                    objpayDescInfo.SpecialAllowPayable = Convert.ToInt32(dr["ACTUALAMOUNT"]);
                }
                if (dr[0].ToString() == "PF")
                {
                    objpayDescInfo.PF = Convert.ToInt32(dr["ACTUALAMOUNT"]);
                }
                if (dr[0].ToString() == "ESI")
                {
                    objpayDescInfo.ESI = Convert.ToInt32(dr["ACTUALAMOUNT"]);
                }
                if (dr[0].ToString() == "WELFARE")
                {
                    objpayDescInfo.WelFare = Convert.ToInt32(dr["ACTUALAMOUNT"]);
                }
                if (dr[0].ToString() == "ADVANCE")
                {
                    objpayDescInfo.Advance = Convert.ToInt32(dr["ACTUALAMOUNT"]);
                }
                if (dr[0].ToString() == "INCOME TAX")
                {
                    objpayDescInfo.IncomeTex = Convert.ToInt32(dr["ACTUALAMOUNT"]);
                }
                if (dr[0].ToString() == "OTHERS DED.")
                {
                    objpayDescInfo.OtherDed = Convert.ToInt32(dr["ACTUALAMOUNT"]);
                }

            }
            objpayDescInfo.DeductionTotal = objpayDescInfo.PF + objpayDescInfo.ESI + objpayDescInfo.WelFare + objpayDescInfo.Advance + objpayDescInfo.IncomeTex + objpayDescInfo.OtherDed;
            return objpayDescInfo;
        }

        public List<LeaveBalanceModels> GetLeaveBalance()
        {
            List<LeaveBalanceModels> lstLBalance = new List<LeaveBalanceModels>();
            string EMP_DOJ = string.Empty;
            string strConn = WebConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            OleDbConnection Conn = new OleDbConnection(strConn);
            OleDbDataReader rdr;
            string sqlUserName;
            double CasualLeave = 0.0;
            double SickLeave = 0.0;
            string EarnLeave = "";
            int CasualleaveGrant = 0;
            decimal EarnleaveGrant = 0;
            int SickleaveGrant = 0;
            int CasualleaveTaken = 0;
            int EarnleaveTaken = 0;
            int SickleaveTaken = 0;
            string Year = DateTime.Now.AddMonths(0).ToString("yyyy");
            int MonthNo = Convert.ToInt32(DateTime.Now.AddMonths(0).ToString("MM"));
            DateTime now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            DateTime yearstartDate = new DateTime(now.Year, 1, 1);
            DateTime lastDate = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1);
            string yearFirstDate = yearstartDate.ToString("dd-MMM-yyyy");
            string MonthLastDate = lastDate.ToString("dd-MMM-yyyy");
            sqlUserName = "SELECT EMP_DOJ FROM PAY_EMPLOYEE_MST WHERE EMP_CODE='" + Session["USER_ID"] + "' AND Company_Code = '" + Session["COMPANY_CODE"] + "'";
            OleDbCommand com = new OleDbCommand(sqlUserName, Conn);
            Conn.Open();
            rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                EMP_DOJ = rdr[0].ToString();
            }
            Conn.Close();

            sqlUserName = "select (SUM(CASE WHEN firsthalf=10 THEN .5 ELSE 0 END ) + SUM(CASE WHEN SECONDhalf=10 THEN .5 ELSE 0 END ))/20 as EarnLeave from pay_attn_MST where EMP_CODE='" + Session["USER_ID"] + "' AND ATTN_DATE>=TO_DATE('" + yearFirstDate + "','DD-MON-YYYY')";
            OleDbCommand com2 = new OleDbCommand(sqlUserName, Conn);
            Conn.Open();
            rdr = com2.ExecuteReader();
            while (rdr.Read())
            {
                EarnLeave = rdr[0].ToString();
            }
            Conn.Close();

            sqlUserName = "SELECT 'OPENING' AS TITLE_DESC,MAX(CASE WHEN LEAVECODE=1 THEN OPENING ELSE 0 END) AS CASUAL_DTL,MAX(CASE WHEN LEAVECODE=2 THEN OPENING ELSE 0 END) AS EARN_DTL,MAX(CASE WHEN LEAVECODE = 3 THEN OPENING ELSE 0 END) AS SICK_DTL FROM PAY_OPLEAVE_TRN WHERE PAYYEAR = '" + Year + "' AND EMP_CODE = '" + Session["USER_ID"] + "'";
            sqlUserName += " UNION ALL ";
            sqlUserName += "SELECT 'ENTITLE' AS TITLE_DESC,MAX(CASE WHEN LEAVECODE=1 THEN TOTENTITLE ELSE 0 END) AS CASUAL_DTL,MAX(CASE WHEN LEAVECODE=2 THEN TOTENTITLE ELSE 0 END) AS EARN_DTL,MAX(CASE WHEN LEAVECODE = 3 THEN TOTENTITLE ELSE 0 END) AS SICK_DTL FROM PAY_LEAVEDTL_MST WHERE PAYYEAR = '" + Year + "' ";
            sqlUserName += " UNION ALL ";
            sqlUserName += "SELECT TO_CHAR(ATTN_DATE,'MON-YY') AS TITLE_DESC,SUM(CASE WHEN FIRSTHALF=1 THEN 0.5 ELSE 0 END + CASE WHEN SECONDHALF=1 THEN 0.5 ELSE 0 END) AS CASUAL_DTL , SUM(CASE WHEN FIRSTHALF = 2 THEN 0.5 ELSE 0 END + CASE WHEN SECONDHALF = 2 THEN 0.5 ELSE 0 END) AS EARN_DTL, SUM(CASE WHEN FIRSTHALF = 3 THEN 0.5 ELSE 0 END + CASE WHEN SECONDHALF = 3 THEN 0.5 ELSE 0 END) AS SICK_DTL";
            sqlUserName += " FROM PAY_ATTN_MST WHERE COMPANY_CODE=" + Session["COMPANY_CODE"] + " AND EMP_CODE ='" + Session["USER_ID"] + "' AND (FIRSTHALF = 2 OR SECONDHALF = 2 ) AND ATTN_DATE>=TO_DATE('" + yearFirstDate + "','DD-MON-YYYY') and ATTN_DATE<=TO_DATE('" + MonthLastDate + "','DD-MON-YYYY') GROUP BY TO_CHAR(ATTN_DATE, 'MON-YY') ";

            Conn.Open();
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            adapter.SelectCommand = new OleDbCommand(sqlUserName, Conn);
            DataSet ds = new DataSet("leaveDetails");
            adapter.Fill(ds);
            DataTable LeaveBalance = ds.Tables[0];
            foreach (DataRow dr in LeaveBalance.Rows)
            {
                LeaveBalanceModels obj1 = new LeaveBalanceModels();
                obj1.Title = dr[0].ToString();
                if (dr[0].ToString() == "OPENING")
                {
                    CasualleaveGrant += Convert.ToInt32(dr[1]);
                    EarnleaveGrant += Convert.ToInt32(dr[2]);
                    SickleaveGrant += Convert.ToInt32(dr[3]);
                    obj1.CasualLeave = Convert.ToInt32(dr[1]);
                    obj1.EarnLeave = Convert.ToInt32(dr[2]);
                    obj1.SickLeave = Convert.ToInt32(dr[3]);
                }
                if (dr[0].ToString() == "ENTITLE")
                {
                    if (Convert.ToDateTime(EMP_DOJ) < Convert.ToDateTime(yearFirstDate))
                    {
                        CasualLeave = Convert.ToDouble(dr[1]) / 12;
                        SickLeave = Convert.ToDouble(dr[3]) / 12;
                        obj1.CasualLeave = Convert.ToInt32(CasualLeave * MonthNo);
                        CasualleaveGrant += Convert.ToInt32(CasualLeave * MonthNo);
                        obj1.EarnLeave = Convert.ToDecimal(EarnLeave);
                        EarnleaveGrant += Convert.ToDecimal(EarnLeave);
                        obj1.SickLeave = Convert.ToInt32(SickLeave * MonthNo);
                        SickleaveGrant += Convert.ToInt32(SickLeave * MonthNo);
                    }
                    else
                    {
                        CasualLeave = Convert.ToDouble(dr[1]) / 12;
                        SickLeave = Convert.ToDouble(dr[3]) / 12;
                        int DojMonth = Convert.ToInt32(Convert.ToDateTime(EMP_DOJ).ToString("MM"));
                        obj1.CasualLeave = Convert.ToInt32(CasualLeave * (MonthNo - DojMonth));
                        CasualleaveGrant += Convert.ToInt32(CasualLeave * (MonthNo - DojMonth));
                        obj1.EarnLeave = Convert.ToDecimal(dr[2]);
                        EarnleaveGrant += Convert.ToDecimal(dr[2]);
                        obj1.CasualLeave = Convert.ToInt32(SickLeave * (MonthNo - DojMonth));
                        SickleaveGrant += Convert.ToInt32(SickLeave * (MonthNo - DojMonth));
                    }
                }
                if (dr[0].ToString() != "OPENING" && dr[0].ToString() != "ENTITLE")
                {
                    CasualleaveTaken += Convert.ToInt32(dr[1]);
                    EarnleaveTaken += Convert.ToInt32(dr[2]);
                    SickleaveTaken += Convert.ToInt32(dr[3]);
                    obj1.CasualLeave = Convert.ToInt32(dr[1]);
                    obj1.EarnLeave = Convert.ToDecimal(dr[2]);
                    obj1.SickLeave = Convert.ToInt32(dr[3]);
                }
                lstLBalance.Add(obj1);
            }
            ViewBag.TotalCasualValue = (CasualleaveGrant - CasualleaveTaken);
            ViewBag.TotalEarnValue = Convert.ToDecimal(EarnleaveGrant - EarnleaveTaken);
            ViewBag.TotalSickValue = (SickleaveGrant - SickleaveTaken);
            return lstLBalance;
        }

        public string GetLastDateOfMonth(int year, int month)
        {
            string result = "";
            int daysInMonth = DateTime.DaysInMonth(year, month);
            DateTime lastDate = new DateTime(year, month, daysInMonth);
            result = lastDate.ToString("dd-MMM-yyyy");
            return result;
        }

        [HttpPost]
        public JsonResult GetCurrentMonthAttendance()
        {
            List<CurrentMonthAttendanceModels> lstCMAttendance = new List<CurrentMonthAttendanceModels>();
            int FilterDate = Convert.ToInt32(WebConfigurationManager.AppSettings.Get("FilterDate"));
            string EMP_DOJ = string.Empty;
            string sqlUserName;
            string MonthName;
            string strConn = WebConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
            OleDbConnection Conn = new OleDbConnection(strConn);
            int CurrentDate =Convert.ToInt32(DateTime.Now.AddMonths(0).ToString("dd"));
            if(CurrentDate< FilterDate)
            {
                MonthName = DateTime.Now.AddMonths(-1).ToString("MMMM-yyyy").ToUpper();
            }
            else
            {
                MonthName = DateTime.Now.AddMonths(0).ToString("MMMM-yyyy").ToUpper();
            }
            string datefilter = MonthName;
            sqlUserName = "select A.ATTN_DATE,to_char(A.IN_TIME,'HH24:Mi'),to_char(A.OUT_TIME,'HH24:Mi'),B.FIRSTHALF,B.SECONDHALF from PAY_DALIY_ATTN_TRN A,PAY_ATTN_MST B WHERE A.COMPANY_CODE=b.COMPANY_CODE AND A.ATTN_DATE = B.ATTN_DATE AND A.Company_Code = '" + Session["COMPANY_CODE"] + "' AND B.Company_Code = '" + Session["COMPANY_CODE"] + "' AND A.EMP_CODE ='" + Session["USER_ID"] + "' AND B.EMP_CODE ='" + Session["USER_ID"] + "' AND TO_CHAR(A.ATTN_DATE,'MON-YYYY') = '" + datefilter + "' AND TO_CHAR(B.ATTN_DATE,'MON-YYYY') = '" + datefilter + "'";
            Conn.Open();
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            adapter.SelectCommand = new OleDbCommand(sqlUserName, Conn);
            DataSet ds = new DataSet("CurrentMonthAttendanceData");
            adapter.Fill(ds);
            DataTable CurrentMonthData = ds.Tables[0];
            foreach (DataRow dr in CurrentMonthData.Rows)
            {
                CurrentMonthAttendanceModels obj1 = new CurrentMonthAttendanceModels();
                obj1.AttnDate = Convert.ToDateTime(dr[0]).ToString("dd");
                obj1.InTime = dr[1].ToString();
                obj1.OutTime = dr[2].ToString();
                obj1.FirstHalf = Convert.ToInt32(dr[3]);
                obj1.SecondHalf = Convert.ToInt32(dr[4]);
                lstCMAttendance.Add(obj1);
            }
            Conn.Close();
            return Json(lstCMAttendance);
        }

        public string ConvertStringToHex(string sText)
        {

            StringBuilder hexDump = new StringBuilder();

            foreach (char c in sText)
            {
                hexDump.AppendFormat("{0:X2}", (int)c);
            }

            return hexDump.ToString();

            //int lIdx;
            //string ToHexDump = "";
            //StringBuilder s = new StringBuilder();
            //for (int i=0;i< sText.Length;i++)
            //{
            //    char data = sText[i];
            //    byte[] bytes = Encoding.UTF8.GetBytes((data).ToString());
            //    var hexString = BitConverter.ToString(bytes);
            //    s.Append(hexString);
            //}
            //ToHexDump = s.ToString();
            //return ToHexDump;

            //byte[] bytes = Encoding.UTF8.GetBytes(sText);
            //var hexString = BitConverter.ToString(bytes);
            //hexString = hexString.Replace("-", "");
            //ToHexDump = Convert.ToString(hexString);
            //return ToHexDump;

            //string hexString = Convert.ToHexString(bytes);
            //byte[] ba = System.Text.Encoding.Default.GetBytes(sText);
            //var hexString = BitConverter.ToString(ba);
            //hexString = hexString.Replace("-", "");

            //int lIdx;
            //string ToHexDump = "";
            //for (lIdx = 1; lIdx <= sText.Length; lIdx++)
            //{
            //    string TempString;

            //    TempString = "0" & Hex(Asc(sText.Substring(lIdx, 1)));
            //    ToHexDump = ToHexDump & TempString.Substring(TempString.Length - 2, 2);
            //    //ToHexDumpresult = ToHexDumpresult + Microsoft.VisualBasic.Strings.Right(("0" + Conversion.Hex(Strings.Asc(Strings.Mid(sText, lIdx, 1))), 2);
            //}
        }

        public string ConvertHextoString(string hexValue)
        {
            // Hexadecimal value to be converted
            //string hexValue = "48656C6C6F20576F726C64"; // Hex representation of "Hello World"

            // Convert hex to byte array
            byte[] byteData = new byte[hexValue.Length / 2];
            for (int i = 0; i < byteData.Length; i++)
            {
                byteData[i] = Convert.ToByte(hexValue.Substring(i * 2, 2), 16);
            }

            // Convert byte array to string
            string stringValue = System.Text.Encoding.UTF8.GetString(byteData);
            return stringValue;
        }

    }
}