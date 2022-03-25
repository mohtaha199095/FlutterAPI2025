using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace MT_Soft_2.DAL
{
    public static class clsConvertNumberToString
    {

        public static string NoToTxt(double TheNo)
        {
            string MyCur = "دينار";
            string MySubCur = "فلس"; bool Dec3Digit = true;
            string NoToTxtRet = "";
            // ======================
            if (Dec3Digit)
            {
                if (TheNo > 999999999999.999)
                {
                    NoToTxtRet = "";
                    return NoToTxtRet;
                }
            }
            else if (TheNo > 999999999999.99)
            {
                NoToTxtRet = "";
                return NoToTxtRet;
            }
            // ======================
            if (TheNo == 0)
            {
                NoToTxtRet = "صفر";
                return NoToTxtRet;
            }
            // ======================

            string MyNo, GetNo, RdNo, GetTxt, ReMark = "";
            string My100, My10, My1, My11 = "", My12 = "";
            string Mybillion = "", MyMillion = "";
            string MyThou = "", MyHun = "", MyFraction = "";
            var MyArry1 = new string[10];
            var MyArry2 = new string[10];
            var MyArry3 = new string[10];
            string MyAnd;
            // ======================
            MyAnd = " و";
            MyArry1[0] = "";
            MyArry1[1] = "واحد";
            MyArry1[2] = "إثنان";
            MyArry1[3] = "ثلاثة";
            MyArry1[4] = "أربعة";
            MyArry1[5] = "خمسة";
            MyArry1[6] = "ستة";
            MyArry1[7] = "سبعة";
            MyArry1[8] = "ثمانية";
            MyArry1[9] = "تسعة";
            MyArry2[0] = "";
            MyArry2[1] = " عشر";
            MyArry2[2] = "عشرون";
            MyArry2[3] = "ثلاثون";
            MyArry2[4] = "أربعون";
            MyArry2[5] = "خمسون";
            MyArry2[6] = "ستون";
            MyArry2[7] = "سبعون";
            MyArry2[8] = "ثمانون";
            MyArry2[9] = "تسعون";
            MyArry3[0] = "";
            MyArry3[1] = "مئة";
            MyArry3[2] = "مئتان";
            MyArry3[3] = "ثلاثمائة";
            MyArry3[4] = "اربعمائة";
            MyArry3[5] = "خمسمائة";
            MyArry3[6] = "ستمائة";
            MyArry3[7] = "سبعمائة";
            MyArry3[8] = "تمانمائة";
            MyArry3[9] = "تسعمائة";
            // ======================
            if (Dec3Digit)
            {
                GetNo = Strings.Format(TheNo, "000000000000.000");
            }
            else
            {
                GetNo = Strings.Format(TheNo, "000000000000.00");
            }
            // ======================
            int i;
            for (i = 0; i <= 14; i += 3)
            {
                if (i < 12)
                {
                    MyNo = Strings.Mid(GetNo, i + 1, 3);
                }
                else if (Dec3Digit)
                {
                    MyNo = Strings.Mid(GetNo, i + 2, 3);
                }
                else
                {
                    MyNo = "0" + Strings.Mid(GetNo, i + 2, 3);
                }

                if (Conversions.ToDouble(Strings.Mid(MyNo, 1, 3)) > 0)
                {
                    RdNo = Strings.Mid(MyNo, 1, 1);
                    My100 = MyArry3[Conversions.ToInteger(RdNo)];
                    RdNo = Strings.Mid(MyNo, 3, 1);
                    My1 = MyArry1[Conversions.ToInteger(RdNo)];
                    RdNo = Strings.Mid(MyNo, 2, 1);
                    My10 = MyArry2[Conversions.ToInteger(RdNo)];
                    if (Conversions.ToDouble(Strings.Mid(MyNo, 2, 2)) == 11)
                        My11 = "أحد عشر";
                    if (Conversions.ToDouble(Strings.Mid(MyNo, 2, 2)) == 12)
                        My12 = "أثنا عشر";
                    if (Conversions.ToDouble(Strings.Mid(MyNo, 2, 2)) == 10)
                        My10 = "عشرة";
                    if (Conversions.ToDouble(Strings.Mid(MyNo, 1, 1)) > 0 & Conversions.ToDouble(Strings.Mid(MyNo, 2, 2)) > 0)
                        My100 = My100 + MyAnd;
                    if (Conversions.ToDouble(Strings.Mid(MyNo, 3, 1)) > 0 & Conversions.ToDouble(Strings.Mid(MyNo, 2, 1)) > 1)
                        My1 = My1 + MyAnd;
                    GetTxt = My100 + My1 + My10;
                    if (Conversions.ToDouble(Strings.Mid(MyNo, 3, 1)) == 1 & Conversions.ToDouble(Strings.Mid(MyNo, 2, 1)) == 1)
                    {
                        GetTxt = My100 + My11;
                        if (Conversions.ToDouble(Strings.Mid(MyNo, 1, 1)) == 0)
                            GetTxt = My11;
                    }

                    if (Conversions.ToDouble(Strings.Mid(MyNo, 3, 1)) == 2 & Conversions.ToDouble(Strings.Mid(MyNo, 2, 1)) == 1)
                    {
                        GetTxt = My100 + My12;
                        if (Conversions.ToDouble(Strings.Mid(MyNo, 1, 1)) == 0)
                            GetTxt = My12;
                    }

                    if (i == 0 & !string.IsNullOrEmpty(GetTxt))
                    {
                        if (Conversions.ToDouble(Strings.Mid(MyNo, 1, 3)) > 10)
                        {
                            Mybillion = GetTxt + " مليون";
                        }
                        else
                        {
                            Mybillion = GetTxt + " مليون";
                            if (Conversions.ToDouble(Strings.Mid(MyNo, 1, 3)) == 2)
                                Mybillion = " مليون";
                            if (Conversions.ToDouble(Strings.Mid(MyNo, 1, 3)) == 2)
                                Mybillion = " ملايين";
                        }
                    }

                    if (i == 3 & !string.IsNullOrEmpty(GetTxt))
                    {
                        if (Conversions.ToDouble(Strings.Mid(MyNo, 1, 3)) > 10)
                        {
                            MyMillion = GetTxt + " مليون";
                        }
                        else
                        {
                            MyMillion = GetTxt + " ملايين";
                            if (Conversions.ToDouble(Strings.Mid(MyNo, 1, 3)) == 1)
                                MyMillion = " مليون";
                            if (Conversions.ToDouble(Strings.Mid(MyNo, 1, 3)) == 2)
                                MyMillion = " ملايين";
                        }
                    }

                    if (i == 6 & !string.IsNullOrEmpty(GetTxt))
                    {
                        if (Conversions.ToDouble(Strings.Mid(MyNo, 1, 3)) > 10)
                        {
                            MyThou = GetTxt + " ألف";
                        }
                        else
                        {
                            MyThou = GetTxt + " آلاف";
                            if (Conversions.ToDouble(Strings.Mid(MyNo, 3, 1)) == 1)
                                MyThou = " ألف";
                            if (Conversions.ToDouble(Strings.Mid(MyNo, 3, 1)) == 2)
                                MyThou = " آلاف";
                        }
                    }

                    if (i == 9 & !string.IsNullOrEmpty(GetTxt))
                        MyHun = GetTxt;
                    if (i == 12 & !string.IsNullOrEmpty(GetTxt))
                        MyFraction = GetTxt;
                }
            }

            if (!string.IsNullOrEmpty(Mybillion))
            {
                if (!string.IsNullOrEmpty(MyMillion) | !string.IsNullOrEmpty(MyThou) | !string.IsNullOrEmpty(MyHun))
                    Mybillion = Mybillion + MyAnd;
            }

            if (!string.IsNullOrEmpty(MyMillion))
            {
                if (!string.IsNullOrEmpty(MyThou) | !string.IsNullOrEmpty(MyHun))
                    MyMillion = MyMillion + MyAnd;
            }

            if (!string.IsNullOrEmpty(MyThou))
            {
                if (!string.IsNullOrEmpty(MyHun))
                    MyThou = MyThou + MyAnd;
            }

            if (!string.IsNullOrEmpty(MyFraction))
            {
                if (!string.IsNullOrEmpty(Mybillion) | !string.IsNullOrEmpty(MyMillion) | !string.IsNullOrEmpty(MyThou) | !string.IsNullOrEmpty(MyHun))
                {
                    NoToTxtRet = ReMark + Mybillion + MyMillion + MyThou + MyHun + " " + MyCur + MyAnd + MyFraction + " " + MySubCur;
                }
                else
                {
                    NoToTxtRet = ReMark + MyFraction + " " + MySubCur;
                }
            }
            else
            {
                NoToTxtRet = ReMark + Mybillion + MyMillion + MyThou + MyHun + " " + MyCur;
            }

            return NoToTxtRet;
        }
    }
}