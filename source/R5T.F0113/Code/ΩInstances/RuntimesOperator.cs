using System;


namespace R5T.F0113
{
    public class RuntimesOperator : IRuntimesOperator
    {
        #region Infrastructure

        public static IRuntimesOperator Instance { get; } = new RuntimesOperator();


        private RuntimesOperator()
        {
        }

        #endregion
    }
}
