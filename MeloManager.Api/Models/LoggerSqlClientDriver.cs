using System;
using System.Data;
using System.Linq;
using NHibernate.Driver;
using NLog;

namespace MeloManager.Api.Models
{
    public class LoggerSqlClientDriver : SqlClientDriver
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override void AdjustCommand(IDbCommand command)
        {
            //log here
            //log.Trace(ToString(command));
            base.AdjustCommand(command);
        }

        //protected override void OnBeforePrepare(IDbCommand command)
        //{
        //    //log here
        //    base.OnBeforePrepare(command);
        //}

        public static bool IsNumeric(object expression)
        {
            if (expression == null || expression is DateTime)
                return false;

            if (expression is Int16 || expression is Int32 || expression is Int64
                || expression is Decimal || expression is Single || expression is Double
                /*|| Expression is Boolean*/)
                return true;

            try
            {
                var s = expression as string;
                Double.Parse(s ?? expression.ToString());
                return true;
            }
            catch
            {
            } // just dismiss errors but return false
            return false;
        }

        internal static string ToString(IDbCommand command)
        {
            var scr = new System.Text.StringBuilder();
            for (int i = 0; i <= command.Parameters.Count - 1; i++)
            {
                var p = (IDbDataParameter) command.Parameters[i];

                string prec = "";
                switch (p.DbType)
                {
                    case DbType.Decimal:
                        prec = string.Format("({0},{1})", p.Precision, p.Scale);
                        break;
                    case DbType.AnsiString:
                    case DbType.String:
                        int size = p.Size;
                        if (size == 0 && (!object.ReferenceEquals(p.Value, DBNull.Value)) && (p.Value != null))
                        {
                            size = p.Value.ToString().Length;
                        }
                        prec = string.Format("({0})", size);
                        break;
                }

                scr.AppendFormat("DECLARE {0} AS {1}{2}; SET {0}=", p.ParameterName,
                    ((System.Data.SqlClient.SqlParameter) p).SqlDbType.ToString().ToLower(), prec);
                if (object.ReferenceEquals(p.Value, DBNull.Value) || (p.Value == null))
                {
                    scr.Append("NULL");
                }
                else if (object.ReferenceEquals(p.Value.GetType(), typeof (bool)))
                {
                    scr.Append(((bool) p.Value ? "1" : "0"));
                }
                else if (true == object.ReferenceEquals(p.Value.GetType(), typeof (DateTime)))
                {
                    scr.AppendFormat("CONVERT(datetime,'{0}',126)", ((DateTime) p.Value).ToString("s"));
                }
                else if (IsNumeric(p.Value) &
                         !object.ReferenceEquals(p.Value.GetType(), typeof (string)))
                {
                    scr.Append(((IFormattable) p.Value).ToString(null,
                        System.Globalization.NumberFormatInfo.InvariantInfo));
                }
                else
                {
                    scr.AppendFormat("'{0}'", p.Value.ToString().Replace("'", "''"));
                }

                scr.Append(";" + Environment.NewLine);
            }

            scr.Append(Environment.NewLine);
            if (command.CommandType == CommandType.Text)
            {
                scr.Append(command.CommandText);
            }
            else if (command.CommandType == CommandType.StoredProcedure)
            {
                int execIndex = scr.Length;
                scr.AppendFormat("EXEC {0} ", command.CommandText);
                var parameters = command.Parameters.Cast<IDbDataParameter>().ToList();
                var retParam = parameters.SingleOrDefault(p => p.Direction == ParameterDirection.ReturnValue);
                scr.Append(string.Join(", ",
                    parameters.Where(p => p.Direction != ParameterDirection.ReturnValue).Select(p =>
                    {
                        switch (p.Direction)
                        {
                            case ParameterDirection.Input:
                                return string.Format("{0}={0}", p.ParameterName);
                            case ParameterDirection.Output:
                            case ParameterDirection.InputOutput:
                                return string.Format("{0}={0} OUT", p.ParameterName);
                        }
                        return null;
                    })));
                if (retParam != null)
                    scr.Insert(execIndex + 5, string.Format("{0} = ", retParam.ParameterName));
            }

            return scr.ToString();
        }
    }
}

