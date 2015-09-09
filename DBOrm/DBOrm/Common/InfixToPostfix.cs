using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBOrm.Common
{
    public class InfixToPostFix
    {
        #region Variables

        private Queue mooperand;
        private Stack mooperator;
        private string moexpression = "";
        private string mopostfixexpression = "";
        private string motokens = "public,private,admin,useradmin";
        private string Con_Operators = "(,),*,+";

        #endregion

        #region Constructors

        public InfixToPostFix(string expression)
        {
            this.InfixExpression = expression;
        }


        public InfixToPostFix(string expression, string tokens)
        {
            this.InfixExpression = expression;
            this.TokensToEvaluate = tokens;
        }


        #endregion

        #region Properties

        private Queue Operands
        {
            get
            {
                if (this.mooperand == null) this.mooperand = new Queue();
                return this.mooperand;
            }
        }


        private Stack Operators
        {
            get
            {
                if (this.mooperator == null) this.mooperator = new Stack();
                return this.mooperator;
            }
        }


        public string InfixExpression
        {
            get { return this.moexpression; }
            set { this.moexpression = value; }
        }


        public string TokensToEvaluate
        {
            get { return this.motokens.Trim().ToLower(); }
            set { this.motokens = value; }
        }


        public string PostFixExpression
        {
            get
            {
                if (this.mopostfixexpression == "")
                {
                    string operand = "";
                    foreach (char ch in InfixExpression)
                    {
                        if (ch == ' ') continue;
                        if (this.IsOperator(ch.ToString()))
                        {
                            if (operand != "")
                            {
                                this.Operands.Enqueue(operand.Trim() + ",");
                                operand = "";
                            }
                            if (ch == '(')
                                this.Operators.Push(ch);
                            else
                                EnqueueOperators(ch);
                        }
                        else
                            operand += ch;
                    }

                    if (operand != "")
                        this.Operands.Enqueue(operand + ",");
                    this.EnqueueOperators(')');

                    while (this.Operands.Count > 0)
                        this.mopostfixexpression += this.Operands.Dequeue().ToString();

                    if (mopostfixexpression.Length > 0 &&
                        mopostfixexpression.Substring(mopostfixexpression.Length - 1).Equals(","))
                        mopostfixexpression = mopostfixexpression.Substring(0, mopostfixexpression.Length - 1);
                }
                this.mooperator = null;
                this.mooperand = null;
                return this.mopostfixexpression;
            }
            set { this.mopostfixexpression = value; }
        }


        #endregion

        #region Methods

        private bool IsOperator(string op)
        {
            return (Con_Operators.IndexOf(op) >= 0);
        }


        private void EnqueueOperators(char op)
        {
            while (this.Operators.Count > 0)
            {
                char ch = Convert.ToChar(this.Operators.Pop());
                if (ch == '(')
                {
                    if (op != ')') this.Operators.Push(ch);
                    break;
                }
                else
                    this.Operands.Enqueue(ch + ",");
            }
            if (op != ')') this.Operators.Push(op);
        }


        public bool EvaluateExpression()
        {
            if (this.PostFixExpression.Trim() != "")
            {
                string[] posfixexp = this.PostFixExpression.Trim().ToLower().Split(',');
                foreach (string exp in posfixexp)
                {
                    if (this.IsOperator(exp))
                    {
                        bool val_0 = Convert.ToBoolean(this.Operators.Pop());
                        bool val_1 = Convert.ToBoolean(this.Operators.Pop());
                        if (exp.Equals("*"))
                            this.Operators.Push((val_1 && val_0));
                        else
                            this.Operators.Push((val_1 || val_0));
                    }
                    else
                    {
                        this.Operators.Push(this.IsValidToken(exp));
                    }
                }
                if (this.Operators.Count > 0)
                    return Convert.ToBoolean(this.Operators.Pop());
            }
            return false;
        }


        private bool IsValidToken(string token)
        {
            return (this.TokensToEvaluate.IndexOf(token.Trim().ToLower()) >= 0);
        }


        #endregion
    }
}
