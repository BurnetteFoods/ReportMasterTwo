using System;
using System.Collections.Generic;
using System.Text;

namespace ReportMasterTwo
{
    public class ExpressionNode
    {
        private string Val;
        public ExpressionNode Left;
        public ExpressionNode Right;
        public ExpressionNode Conditional;
        public ParamInput pm;

        public ExpressionNode(string unp, ParamInput pm)
        {
            this.pm = pm;

            BuildNormalNode(unp);
        }

        public ExpressionNode(string unp, bool conditional, ParamInput pm)
        {
            this.pm = pm;

            if (conditional)
            {
                BuildConditional(unp);
            }
            else
            {
                BuildNormalNode(unp);
            }
        }

        public void BuildConditional(string unp)
        {
            FilterInputString(unp);
            ParseConditional();
        }

        private void FilterInputString(string unp)
        {
            Val = unp;
            Val = Val.TrimStart(null);
            Val = Val.TrimEnd(null);
            Val = Val.ToUpper();
        }

        public void BuildNormalNode(string unp)
        {
            FilterInputString(unp);

            if (Val[0] == '=')
            {
                ParseCalculated();
            }
            else
            {
                Left = null;
                Right = null;
            }
        }

        public List<object> Eval(Dictionary<string, List<object>> map, int recordCount)
        {
            Val = Val.TrimStart(null);
            Val = Val.TrimEnd(null);

            string v = Val.Replace(" ", "");

            if (v.StartsWith("LITERAL:", StringComparison.InvariantCultureIgnoreCase))
            {
                return EvalLiteral(recordCount);
            }
            else if (v.StartsWith("PARAM:", StringComparison.InvariantCultureIgnoreCase))
            {
                return EvalParam(recordCount);
            }
            else if (v[0] == '=')
            {
                if (Left == null && Right == null)
                {
                    v = v.Substring(1).TrimStart(null);

                    if (!map.ContainsKey(v))
                    {
                        return EvalNumeric(recordCount);
                    }
                    else
                    {
                        return (map[v]);
                    }
                }
                else
                {
                    return (EvalRec(map, recordCount));
                }
            }
            else
            {
                if (!map.ContainsKey(v))
                    return (EvalNumeric(recordCount));

                return (map[v]);
            }
        }

        public List<bool> EvalCondition(Dictionary<string, List<object>> map, int recordCount)
        {
            BinaryComparison bc;

            if (Val == "==")
                bc = Equal;
            else if (Val == "=<")
                bc = Lesser;
            else if (Val == "=<=")
                bc = LesserEqual;
            else if (Val == "=>")
                bc = Greater;
            else if (Val == "=>=")
                bc = GreaterEqual;
            else if (Val == "=!=")
                bc = Inequal;
            else
                throw new ArgumentException("Bad parse to EvalCondition: No oper set");

            return(EvalConditionOperator(map, recordCount, bc));   
        }

        public List<bool> EvalConditionOperator(Dictionary<string, List<object>> map, int recordCount, BinaryComparison bc)
        {
            List<object> leftResults = Left.Eval(map, recordCount);
            List<object> rightResults = Right.Eval(map, recordCount);
            List<bool> finalResults = new List<bool>(leftResults.Count);

            for (int i = 0; i < leftResults.Count; i++)
            {
                finalResults.Add(bc(Convert.ToDouble(leftResults[i]), Convert.ToDouble(rightResults[i])));
            }

            return (finalResults);
        }

        public List<object> EvalRec(Dictionary<string, List<object>> map, int recordCount)
        {
            if (Val == "=^")
            {
                return(EvalRecOperator(map, recordCount, Math.Pow));
            }
            else if (Val == "=*")
            {
                return(EvalRecOperator(map, recordCount, Multiply));
            }
            else if (Val == "=/")
            {
                return(EvalRecOperator(map, recordCount, Divide));
            }
            else if (Val == "=+")
            {
                return(EvalRecOperator(map, recordCount, Add));
            }
            else if (Val == "=-")
            {
                return(EvalRecOperator(map, recordCount, Subtract));
            }
            else if (Val == "=IF")
            {
                return (EvalIF(map, recordCount));
            }
            else
                throw new ArgumentException("Invalid Operator in Expression");
        }

        public static bool Lesser(double v1, double v2)
        {
            return (v1 < v2);
        }

        public static bool LesserEqual(double v1, double v2)
        {
            return (v1 <= v2);
        }

        public static bool Equal(double v1, double v2)
        {
            return (v1 == v2);
        }

        public static bool GreaterEqual(double v1, double v2)
        {
            return (v1 >= v2);
        }

        public static bool Greater(double v1, double v2)
        {
            return (v1 > v2);
        }

        public static bool Inequal(double v1, double v2)
        {
            return (v1 != v2);
        }

        public static double Add(double v1, double v2)
        {
            return (v1 + v2);
        }

        public static double Subtract(double v1, double v2)
        {
            return (v1 - v2);
        }

        public static double Multiply(double v1, double v2)
        {
            return (v1 * v2);
        }

        public static double Divide(double v1, double v2)
        {
            return (v1 / v2);
        }

        public List<object> EvalIF(Dictionary<string, List<object>> map, int recordCount)
        {
            if (Conditional == null)
                throw new ArgumentException("Parse Failure on EvalIF");

            List<object> leftResults = Left.Eval(map, recordCount);
            List<object> rightResults = Right.Eval(map, recordCount);
            List<bool> conditionalResults = Conditional.EvalCondition(map, recordCount);
            List<object> finalResults = new List<object>(leftResults.Count);

            for (int i = 0; i < leftResults.Count; i++)
            {
                finalResults.Add((conditionalResults[i] ? leftResults[i] : rightResults[i])); 
            }

            return (finalResults);
        }

        public List<object> EvalRecOperator(Dictionary<string, List<object>> map, int recordCount, BinaryExpression binary)
        {
            List<object> leftResults = Left.Eval(map, recordCount);
            List<object> rightResults = Right.Eval(map, recordCount);
            List<object> finalResults = new List<object>(leftResults.Count);

            for (int i = 0; i < leftResults.Count; i++)
            {
                finalResults.Add(binary(Convert.ToDouble(leftResults[i]), Convert.ToDouble(rightResults[i])));
            }

            return (finalResults);
        }
 
        private List<object> EvalNumeric(int recordCount)
        {
            List<object> temp = new List<object>(recordCount);

            if (Val == "$N")
            {
                for (int i = 0; i < recordCount; i++)
                {
                    temp.Add("");
                }
            }
            else
            {
                double numericVal = 0;

                try
                {
                    numericVal = Convert.ToDouble(Val);
                }
                catch (FormatException)
                {
                    temp.Add(Val);
                }

                for (int i = 0; i < recordCount; i++)
                {
                    temp.Add(numericVal);
                }
            }

            return temp;
        }

        private List<object> EvalLiteral(int recordCount)
        {
            List<object> temp = new List<object>(recordCount);

            // The value is everything after the text LITERAL:, starting at index position 8
            Val = Val.Substring(8);

            for (int i = 0; i < recordCount; i++)
            {
                temp.Add(Val);
            }

            return temp;
        }

        private List<object> EvalParam(int recordCount)
        {
            List<object> temp = new List<object>(recordCount);

            // The paramName is everything after the text PARAM:, starting at index position 6
            string paramName = Val.Substring(6).ToUpper();

            Val = pm.eval(paramName);

            for (int i = 0; i < recordCount; i++)
            {
                temp.Add(Val);
            }

            return temp;
        }

        public void ParseCalculated()
        {
            int calcIn = Val.IndexOf("CALC EXP:");

            if (calcIn != -1)
            {
                Val = "=" + Val.Substring(calcIn + 9);
            }

            if (Val.StartsWith("=IF"))
            {
                ProcessIF();
            }
            // val of whitespace
            else if (Val == "=")
            {
                Val = "Literal: ";
                Left = null;
                Right = null;
            }
            else if (Val.Substring(1).TrimStart(null)[0] == '{')
            {
                Val = Val.Substring(1).TrimStart(null);
                Left = null;
                Right = null;
            }
            else if (Val.IndexOf('(') == -1)
            {
                ParseNonParen();
            }
            else
            {
                ParseParen();
            }
        }

        public void ParseConditional()
        {
            int eqSplitIndex = SplitOnParenClear(Val, '=', '<');
            int grSplitIndex = SplitOnParenClear(Val, '>', '>');

            if (eqSplitIndex == -1 && grSplitIndex == -1)
                throw new ArgumentException("Invalid Conditional: No Condition");

            int finalIndex = (eqSplitIndex >= grSplitIndex ? eqSplitIndex : grSplitIndex);

            char indexChar = Val[finalIndex];

            switch (indexChar)
            {
                case '=':
                    char precChar = Val[finalIndex - 1];

                    if (precChar == '<' || precChar == '>' || precChar == '!')
                        ParsePairTokenOper(finalIndex - 1, finalIndex);
                    else
                        ParseOneTokenOper(finalIndex);
                    break;
                case '>':
                case '<':
                    char postChar = Val[finalIndex + 1];

                    if (postChar == '=')
                        ParsePairTokenOper(finalIndex, finalIndex + 1);
                    else
                        ParseOneTokenOper(finalIndex);
                    break;
                default:
                    throw new ArgumentException("Invalid operator");
            }
        }

        public void ParseOneTokenOper(int index)
        {
            Left = new ExpressionNode("=" + Val.Substring(0, index), pm);
            Right = new ExpressionNode("=" + Val.Substring(index + 1), pm);
            Val = "=" + Val.Substring(index, 1);
        }

        public void ParsePairTokenOper(int startIndex, int endIndex)
        {
            Left = new ExpressionNode("=" + Val.Substring(0, startIndex), pm);
            Right = new ExpressionNode("=" + Val.Substring(endIndex + 1), pm);
            Val = "=" + Val.Substring(startIndex, endIndex - startIndex + 1);
        }

        public void ProcessIF()
        {
            int parenCount = 0;
            bool parenOpened = false;
            int searchIndex = 0;

            while (!parenOpened || parenCount != 0)
            {
                if (Val[searchIndex] == '(')
                {
                    parenCount++;
                    parenOpened = true;
                }
                else if (Val[searchIndex] == ')')
                {
                    parenCount--;
                }

                searchIndex++;
            }

            if (parenCount < 0 || !parenOpened || parenCount > 0)
                throw new ArgumentException("Bad IF statement formatting");

            string v = Val.Substring(searchIndex);

            if (v == null || v.TrimEnd(null).TrimStart(null) == "")
                ProcessPureIF();
            else if (v.IndexOf('-') != -1 || v.IndexOf('+') != -1 || v.IndexOf('*') != -1 || v.IndexOf('/') != -1
                || v.IndexOf('^') != -1)
            {
                ParseParen();
            }
            else
            {
                ProcessPureIF();
            }
        }

        public void ProcessPureIF()
        {
            int parenOpen = Val.IndexOf('(');
           
            // Using SplitOnParenClear to avoid hitting commas in nested IFs
            // And in SQL fns
            int firstComma = parenOpen + 1 + SplitOnParenClear(Val.Substring(parenOpen + 1), ',', ',');

            if (firstComma == -1)
                throw new ArgumentException("Invalid IF syntax. Must use IF(cond , iftrue , iffalse )");

            int secondComma = firstComma + 1 + SplitOnParenClear(Val.Substring(firstComma + 1), ',', ',');

            if(secondComma == -1)
                throw new ArgumentException("Invalid IF syntax. Must use IF(cond , iftrue , iffalse )");

            int parenClose = Val.LastIndexOf(')');

            string condPart = Val.Substring(parenOpen + 1, firstComma - parenOpen - 1);
            string truePart = Val.Substring(firstComma + 1, secondComma - firstComma - 1);
            string falsePart = Val.Substring(secondComma + 1, parenClose - secondComma - 1);

            Conditional = new ExpressionNode(condPart, true, pm);
            Left = new ExpressionNode("=" + truePart.TrimStart(null).TrimEnd(null), pm);
            Right = new ExpressionNode("=" + falsePart.TrimStart(null).TrimEnd(null), pm);
            Val = "=IF";    // Conditional section for eval
        }

        public void ParseParen()
        {
            int splitIndex = SplitOnParenClear(Val, '+', '-');
            BreakVal(splitIndex);

            if (splitIndex != -1)
                return;

            splitIndex = SplitOnParenClear(Val, '*', '/');
            BreakVal(splitIndex);

            if (splitIndex != -1)
                return;

            splitIndex = SplitOnParenClear(Val, '^', '^');
            BreakVal(splitIndex);

            if (splitIndex != -1)
                return;

            ParsePureParen();
        }

        public void ParsePureParen()
        {
            int lparanIndex = -1;
            int activateIndex = -1;
            int paranNum = 0;
            bool paranActivate = false;

            for (int i = 0; i < Val.Length; i++)
            {
                if (Val[i] == '(')
                {
                    if (lparanIndex == -1)
                        lparanIndex = i;

                    paranActivate = true;
                    paranNum++;
                }

                if (Val[i] == ')')
                {
                    paranNum--;
                }

                if (paranNum == 0 && paranActivate)
                {
                    activateIndex = i;
                    break;
                }
            }

            Val = "=" + Val.Substring(lparanIndex + 1, (activateIndex - 1) - (lparanIndex + 1) + 1);
            ParseCalculated();
        }

        public void ParseNonParen()
        {
            int plusIndex = Val.IndexOf('+');
            int subIndex = Val.IndexOf('-');

            if (plusIndex == -1 && subIndex == -1)
            {
                int multIndex = Val.IndexOf('*');
                int divIndex = Val.IndexOf('/');

                if (multIndex == -1 && divIndex == -1)
                {
                    int expIndex = Val.IndexOf('^');

                    if (expIndex == -1)
                    {
                        Left = null;
                        Right = null;
                        Val = Val.Substring(1);
                    }

                    BreakVal(expIndex);
                }

                if (multIndex == -1)
                    BreakVal(divIndex);
                else if (divIndex == -1)
                    BreakVal(multIndex);

                if (multIndex < divIndex)
                {
                    BreakVal(multIndex);
                }
                else
                {
                    BreakVal(divIndex);
                }
            }

            if (plusIndex == -1)
                BreakVal(subIndex);
            else if (subIndex == -1)
                BreakVal(plusIndex);

            if (plusIndex < subIndex)
            {
                BreakVal(plusIndex);
            }
            else
            {
                BreakVal(subIndex);
            }
        }

        public static int SplitOnParenClear(string val, char char1, char char2)
        {
            int parenNumber = 0;
            int splitIndex = -1;

            for (int i = 0; i < val.Length; i++)
            {
                if (val[i] == '(')
                    parenNumber++;

                if (val[i] == ')')
                    parenNumber--;

                if (parenNumber == 0)
                {
                    if (val[i] == char1)
                    {
                        splitIndex = i;
                        break;
                    }
                    else if (val[i] == char2)
                    {
                        splitIndex = i;
                        break;
                    }
                }
            }

            return splitIndex;
        }

        public void BreakVal(int index)
        {
            if (index != -1)
            {
                Left = new ExpressionNode("=" + Val.Substring(0, index).TrimStart(null).TrimEnd(null), pm);
                Right = new ExpressionNode("=" + Val.Substring(index + 1).TrimStart(null).TrimEnd(null), pm);
                Val = "=" + Val[index];
            }
        }
    }

    public delegate double BinaryExpression(double val1, double val2);

    public delegate bool BinaryComparison(double val1, double val2);
}
