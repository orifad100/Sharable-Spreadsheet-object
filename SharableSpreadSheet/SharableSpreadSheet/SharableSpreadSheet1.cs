using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;



namespace SharableSpreadSheet
{
    class Program
    {
        static void Main(string[] args)
        {



        }
    }
 
    public  class SharableSpreadSheet1
    {
        public static int rowS;
        public static int columnS;
        static int maxusers = 0;
        private string[,] matrix;
        private List<Mutex> RowMutexWriter = new List<Mutex>();
        private List<bool> Rowbool = new list<bool>();
        private List<bool> Columbool = new list<bool>();
        private List<Mutex> ColumnMutexWriter = new List<Mutex>();
        private List<SemaphoreSlim> RowSemaphoresRead = new List<SemaphoreSlim>();
        private List<SemaphoreSlim> ColumnSemaphoresRead = new List<SemaphoreSlim>();
       

        public SharableSpreadSheet1(int nRows, int nCols, int nUsers =-1)
        {
            
            matrix = new string[nRows, nCols];
            rowS = nRows;
            columnS = nCols;
           // setConcurrentSearchLimit(22);
            if (maxusers == -1)
            {
                maxusers = int.MaxValue;
            }
            maxusers = nUsers;

            
            for (int i = 0; i < rowS; i++)
            {
                Mutex m = new Mutex();
                RowMutexWriter.Add(m);
                SemaphoreSlim s = new SemaphoreSlim( maxusers);
                RowSemaphoresRead.Add(s);
                Rowbool.Add(false);
            }

            for (int i = 0; i < columnS; i++)
            {
                Mutex m = new Mutex();
                ColumnMutexWriter.Add(m);
                SemaphoreSlim s = new SemaphoreSlim(maxusers);
                ColumnSemaphoresRead.Add(s);
                Columbool.Add(false);
            }
        }

        public int getrows()
        {
            return rowS;
    }

        public String getCell(int row, int col)
        {
            while (Rowbool[row]  && Columbool[col] ) { };
            RowSemaphoresRead[row].Wait();
            ColumnSemaphoresRead[col].Wait();
            String str = matrix[row , col ];
            RowSemaphoresRead[row].Release();
            ColumnSemaphoresRead[col].Release();
            return str;
        }
        public void setCell(int row, int col, String str)
        {
            while (RowSemaphoresRead[row].CurrentCount != maxusers && ColumnSemaphoresRead[col].CurrentCount != maxusers) { };//untill we didnt finished all readers action on the specific row/column we dont write
            RowMutexWriter[row].WaitOne();
            ColumnMutexWriter[col].WaitOne();

            Rowbool[row] = true;
            Columbool[col] = true;

            matrix[row, col] = str;

            RowMutexWriter[row].ReleaseMutex();
            ColumnMutexWriter[col].ReleaseMutex();

            Rowbool[row] = false;
            Columbool[col] = false;

        }
        public Tuple<int, int> searchString(String str)  
        {
            try
            {
                int row = 0;
                int col = 0;
                int i, j;
                for (i = 0; i < rowS; i++)
                {
                    RowSemaphoresRead[i].Wait();

                    for (j = 0; j < columnS; j++)
                    {
                        while (Rowbool[i] != false && Columbool[j] != false) { };

                        ColumnSemaphoresRead[j].Wait();
                        if (string.Equals(matrix[i, j], str))
                        {

                            row = i;
                            col = j;
                            RowSemaphoresRead[i].Release();
                            ColumnSemaphoresRead[j].Release();
                            return Tuple.Create(row, col);

                        }

                        ColumnSemaphoresRead[j].Release();
                    }
                    RowSemaphoresRead[i].Release();
                }
                throw new Exception("String does not found .");
            }
            catch(Exception e)
            {
                Console.WriteLine( e);//print the exception
             
            }
            return null;

           
        }
        public void exchangeRows(int row1, int row2)
        {
            while (RowSemaphoresRead[row1].CurrentCount != maxusers && RowSemaphoresRead[row2].CurrentCount != maxusers) { }

            RowMutexWriter[row1].WaitOne();
            RowMutexWriter[row2].WaitOne();

            Rowbool[row1] = true;
            Rowbool[row2] = true;


            String[] temp = new string[columnS];
            for (int i = 0; i < columnS; i++)
            {
                temp[i] += matrix[row1, i];
                if (temp[i] == null) { temp[i] = ""; }
                matrix[row1, i] = matrix[row2, i];
            }

            for (int j = 0; j < columnS; j++)
                matrix[row2, j] = temp[j];

            RowMutexWriter[row1].ReleaseMutex();
            RowMutexWriter[row2].ReleaseMutex();

            Rowbool[row1] = false;
            Rowbool[row2] = false;
        }
        public void exchangeCols(int col1, int col2)
        {
            while (ColumnSemaphoresRead[col1].CurrentCount != maxusers && ColumnSemaphoresRead[col2].CurrentCount != maxusers) { }

            ColumnMutexWriter[col1].WaitOne();
            ColumnMutexWriter[col2].WaitOne();

            Columbool[col1] = true;
            Columbool[col2] = true;

            String[] temp = new string[rowS];
            for (int i = 0; i < rowS; i++)
            {
                temp[i] = matrix[i, col1];
                if(temp[i] == null) { temp[i] = ""; }
                matrix[i, col1] = matrix[i, col2];
            }

            for (int j = 0; j < rowS; j++)
                matrix[j, col2] = temp[j];

            ColumnMutexWriter[col1].ReleaseMutex();
            ColumnMutexWriter[col2].ReleaseMutex();

            Columbool[col1] = false;
            Columbool[col2] = false;

        }
        public int searchInRow(int row, String str)
        {
            try
            {
                int col = 0;
                while (Rowbool[row]) { };
                RowSemaphoresRead[row].Wait();
                for (int i = 0; i < columnS; i++)
                {
                    while (Columbool[i]) { };


                    ColumnSemaphoresRead[i].Wait();

                    if (string.Equals(matrix[row, i], str))
                    {
                        col = i;
                        RowSemaphoresRead[row].Release();
                        ColumnSemaphoresRead[i].Release();
                        return col;

                    }

                    ColumnSemaphoresRead[i].Release();
                }
                RowSemaphoresRead[row].Release();
                throw new Exception("String does not found.");

            }

            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return -1;
         
        }
        public int searchInCol(int col, String str)
        {
            try
            {
                int row = 0;
                while (Columbool[col]) { };
                ColumnSemaphoresRead[col].Wait();

                for (int i = 0; i < rowS; i++)
                {
                    while (Rowbool[i]) { };
                    RowSemaphoresRead[i].Wait();
                    if (string.Equals(matrix[i, col], str))
                    {
                        row = i;
                        ColumnSemaphoresRead[col].Release();
                        RowSemaphoresRead[i].Release();
                        return row;
                    }
                    RowSemaphoresRead[i].Release();
                }
                ColumnSemaphoresRead[col].Release();

                throw new Exception("String does not found .");

            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return -1;
        }
        public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, String str)
        {
            try
            {
                int row = 0, col = 0;
                for (int i = row1; i < row2 + 1; i++)
                {
                    while (Rowbool[i]) { };
                    RowSemaphoresRead[i].Wait();


                    for (int j = col1; j < col2 + 1; j++)
                    {
                        while (Columbool[j]) { };
                        ColumnSemaphoresRead[j].Wait();
                        if (string.Equals(matrix[i, j], str))
                        {
                            row = i;
                            col = j;
                            RowSemaphoresRead[i].Release();
                            ColumnSemaphoresRead[j].Release();
                            return Tuple.Create(row, col);
                        }
                        ColumnSemaphoresRead[j].Release();
                    }
                    RowSemaphoresRead[i].Release();


                }
                throw new Exception("String does not found .");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
        public void addRow(int row1)
        {
            
            String[,] table = new string[rowS + 1, columnS];
            rowS = rowS + 1;
            copy_matrix(table, matrix, 1);
            matrix = table;
            Mutex mu = new Mutex();
            RowMutexWriter.Add(mu);
            Rowbool.Add(false);
            SemaphoreSlim sem = new SemaphoreSlim(maxusers);
            RowSemaphoresRead.Add(sem);


            if (row1 != rowS - 2)
            {
                for (int i = rowS - 1; i > row1 + 1; i--)
                {
                    exchangeRows(i, i - 1);
                }
            }
            else
            {
                while (RowSemaphoresRead[row1 + 1].CurrentCount != maxusers) { }
                RowMutexWriter[row1 + 1].WaitOne();
                Rowbool[row1+1] = true;
             
                for (int j = 0; j < columnS; j++)
                    matrix[row1 + 1, j] = "";

                RowMutexWriter[row1 + 1].ReleaseMutex();
                Rowbool[row1+1] = false;
         

            }

        }
        public void addCol(int col1)
        {
            
            String[,] table = new string[rowS, columnS + 1];
            columnS = columnS + 1;
            copy_matrix(table, matrix, 0);
            matrix = table;
            Mutex mu = new Mutex();
            ColumnMutexWriter.Add(mu);
            Columbool.Add(false);
            SemaphoreSlim sem = new SemaphoreSlim(maxusers);
            ColumnSemaphoresRead.Add(sem);

            if (col1 != columnS - 2)
            {
                Columbool[col1+1] = true;
                

                for (int i = columnS - 1; i > col1 + 1; i--)
                {

                    exchangeCols(i, i - 1);
                }
            }
            else
            {
                while (ColumnSemaphoresRead[col1 + 1].CurrentCount != maxusers) { }
                ColumnMutexWriter[col1 + 1].WaitOne();
                Columbool[col1 + 1] = true;

                for (int j = 0; j < rowS; j++)
                    matrix[j, col1 + 1] = "";

                ColumnMutexWriter[col1 + 1].ReleaseMutex();
                Columbool[col1 + 1] = false;

            }
        }

        public Tuple<int, int>[] findAll(String str, bool caseSensitive)
        {
            
            List<Tuple<int, int>> temptlist = new List<Tuple<int, int>>();
            for (int i = 0; i < rowS; i++)
            {
                while(Rowbool[i] != false) { };
                RowSemaphoresRead[i].Wait();
          
                for (int j = 0; j < columnS; j++)
                {
                    while ( Columbool[j] != false) { };
                    ColumnSemaphoresRead[j].Wait();
                             
                    if (caseSensitive)
                    {
                        if (matrix[i, j] == str)
                        {
                            temptlist.Insert(temptlist.Count, Tuple.Create(i, j));
                        }
                    }
                    else
                    {
                        if (matrix[i, j] == null)
                            matrix[i, j] += "";

                        if (matrix[i, j].Equals(str, StringComparison.CurrentCultureIgnoreCase))
                        {
                            temptlist.Insert(temptlist.Count, Tuple.Create(i, j));
                        }
                    }

                    ColumnSemaphoresRead[j].Release();
               

                }
                RowSemaphoresRead[i].Release();
              

            }
            try
            {
                if (temptlist.Count == 0)
                {
                    throw new Exception("No match .");
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
           
            Tuple<int, int>[] tuparr = new Tuple<int, int>[temptlist.Count];
            for (int run = 0; run < temptlist.Count; run++)
            {
                tuparr[run] = temptlist[run];
            }
            return tuparr;

        }

        public void setAll(String oldStr, String newStr, bool caseSensitive)
        {
            for (int i = 0; i < rowS; i++)
            {
                while (RowSemaphoresRead[i].CurrentCount != maxusers) { };
                RowMutexWriter[i].WaitOne();
                for (int j = 0; j < columnS; j++)
                {
                    while (ColumnSemaphoresRead[j].CurrentCount != maxusers) { };
                    ColumnMutexWriter[j].WaitOne();
                    if (caseSensitive == true)
                    {
                        if (matrix[i, j] == oldStr)
                        {
                            matrix[i, j] = newStr;
                        }
                    }
                    else
                    {
                        if (matrix[i, j] == null)
                            matrix[i, j] += "";

                        if (matrix[i, j].Equals(oldStr, StringComparison.CurrentCultureIgnoreCase))
                        {
                            matrix[i, j] = newStr;
                        }

                    }
                    ColumnMutexWriter[j].ReleaseMutex();


                }
                RowMutexWriter[i].ReleaseMutex();

            }

        }
        public Tuple<int, int> getSize()
        {
            
            int nRows = 0, nCols = 0;

            nRows = rowS; nCols = columnS;
            return Tuple.Create(nRows, nCols);
        }
        public void setConcurrentSearchLimit(int nUsers)
        {
            
            maxusers = nUsers;

        }



        public void save(String fileName)
        {

            StreamWriter sw = new StreamWriter(fileName);

            for (int i = 0; i < rowS; i++)
            {
                for (int j = 0; j < columnS; j++)
                {
                    if (j == columnS - 1)
                        sw.Write(getCell(i, j));
                    else
                        sw.Write(getCell(i, j)+',');
                }
                sw.WriteLine();
            }
            sw.Close();


        }


        public void load(String fileName)
        {
            try
            {
                StreamReader re = new StreamReader(fileName);
                int k = 0;
                for (int i = 0; i < rowS; i++)
                {
                    string line = re.ReadLine();
                    for (int j = 0; j < columnS; j++)
                    {
                        setCell(i, j, line[k].ToString());
                        k = k + 2;
                    }

                }
                re.Close();

            }
            catch(Exception e)
            {
                Console.WriteLine("Could not find file :"+fileName);
            }





        }

        public void copy_matrix(String[,] matrixnew, String[,] matrixold, int flag)
        {
            if (flag == 1)
            {
                for (int i = 0; i < rowS - 1; i++)
                {
                    while (Rowbool[i]) { };
                    RowSemaphoresRead[i].Wait();
                    for (int j = 0; j < columnS; j++)
                    {
                        while (Columbool[j] != false) { };
                        ColumnSemaphoresRead[j].Wait();
                        matrixnew[i, j] = matrixold[i, j];
                        ColumnSemaphoresRead[j].Release();
                    }
                    RowSemaphoresRead[i].Release();
                }
            }

            if (flag == 0)
            {
                for (int i = 0; i < rowS; i++)
                {
                    while (Rowbool[i]) { };
                    RowSemaphoresRead[i].Wait();
                    for (int j = 0; j < columnS - 1; j++)
                    {
                        while (Columbool[j] != false) { };
                        ColumnSemaphoresRead[j].Wait();
                        matrixnew[i, j] = matrixold[i, j];
                        ColumnSemaphoresRead[j].Release();
                    }
                    RowSemaphoresRead[i].Release();
                }
            }
            }


        }

    }
    
