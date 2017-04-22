using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Encodings
{
    public static class ParamsHelper
    {
        /// <summary>
        /// Get application run parameters from command line arguments
        /// </summary>
        /// <param name="args">application command line arguments</param>
        /// <returns></returns>
        public static RunParam GetParams(string[] args)
        {
            RunParam newParams = new RunParam();


            //if input and output are defined
            if (args.Length >= 2)
            {
                newParams.InputFileName = args[0];
                newParams.OutputFileName = args[1];

                for (int i = 2; i < args.Length-1; i++)
                {
                    if (args[i].ToLower().StartsWith("-t"))
                    {
                        int t = 0;
                        if (Int32.TryParse(args[i + 1], out t))
                        {
                            newParams.ThreadsNumber = t;
                        }
                        else
                            throw new ApplicationException("Bad threads number parameter");

                    }
                }

            }
            else
            {
                throw new ApplicationException("not enough parameters are defined");
            }

            if (newParams.ThreadsNumber == 0)
                newParams.ThreadsNumber = 3;

            return newParams;
        }
    }
}
