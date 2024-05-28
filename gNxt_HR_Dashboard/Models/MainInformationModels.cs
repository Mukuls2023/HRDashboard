using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gNxt_HR_Dashboard.Models
{
    public class MainInformationModels
    {
        public LeaveRequestModels LeaveRequest { get; set; }
        public List<MissedPunchModels> lstMissedPunch { get; set; }
        public List<NoticeModels> lstNoticeData { get; set; }
        public string LatestNoticeText { get; set; }
        public string PastNoticeText { get; set; }
        public PaySlipHeaderModels PaySlipHeader { get; set; }
        public PaySlipDescModels PaySlipDesc { get; set; }
        public List<LeaveBalanceModels> lstLeaveBalance { get; set; }
        public int CasualLeaveTotal { get; set; }
        public decimal EarnLeaveTotal { get; set; }
        public int SickLeaveTotal { get; set; }
        public List<CurrentMonthAttendanceModels> lstCurrentMonthAttendance { get; set; }
    }
    public class LeaveRequestModels
    {
        public string LeaveType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime HalfDayDate { get; set; }
        public string leaveTime { get; set; }
        public string LeaveReasion { get; set; }
    }

    public class MissedPunchModels
    {
        public int SNO { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
    }

    public class NoticeModels
    {
        public string NoticeText { get; set; }
    }

    public class PaySlipHeaderModels
    {
        public string MontnName1 { get; set; }
        public string MontnName2 { get; set; }
        public string MontnName3 { get; set; }
        public string MontnNo1 { get; set; }
        public string MontnNo2 { get; set; }
        public string MontnNo3 { get; set; }
        public string Year { get; set; }
        public string EmpCode { get; set; }
        public string Name { get; set; }
        public string PFNo_UIDNo { get; set; }
        public string JoiningDate { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string BankAC { get; set; }
        public string FatherHusbandName { get; set; }
        public string Rate { get; set; }
        public string Payable { get; set; }
        public int BasicSalaryPayable { get; set; }
        public int BasicSalaryRate { get; set; }
        public string SalaryDate { get; set; }
        public string PaySlipMonths { get; set; }

    }

    public class PaySlipDescModels
    {
        public string CurrentDate { get; set; }
        public int HRARate { get; set; }
        public int SpecialAllowRate { get; set; }
        public string IncentiveRate { get; set; }
        public int HRAPayable { get; set; }
        public int SpecialAllowPayable { get; set; }
        public string IncentivePayable { get; set; }
        public int PF { get; set; }
        public int ESI { get; set; }
        public int WelFare { get; set; }
        public int Advance { get; set; }
        public int IncomeTex { get; set; }
        public int OtherDed { get; set; }
        public int GrossSalaryRate { get; set; }
        public int GrossSalaryPayable { get; set; }
        public int NetPay { get; set; }
        public string NetPayInWords { get; set; }
        public int DeductionTotal { get; set; }

    }

    public class LeaveBalanceModels
    {
        public string Title { get; set; }
        public int CasualLeave { get; set; }
        public decimal EarnLeave { get; set; }
        public int SickLeave { get; set; }
    }

    public class CurrentMonthAttendanceModels
    {
        public string AttnDate { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public int FirstHalf { get; set; }
        public int SecondHalf { get; set; }
    }
}