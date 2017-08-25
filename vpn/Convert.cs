using System.Runtime.InteropServices;

namespace Converter
{
  class ConverterHelper
  {
    public static byte[] StructToBytes<T>(T obj)
    {
        int size = Marshal.SizeOf(typeof(T));
           IntPtr bufferPtr = Marshal.AllocHGlobal(size);
        try   
        {
               Marshal.StructureToPtr(obj, bufferPtr, false);
               byte[] bytes = new byte[size];
               Marshal.Copy(bufferPtr, bytes, 0, size);

               return bytes;
        }
                catch(Exception ex)
                {
                    throw new Exception("Error in StructToBytes ! " + ex.Message);
                }
        finally   
        {   
          Marshal.FreeHGlobal(bufferPtr);   
        }  
    }

    //字节流转换成结构体
    public static T BytesToStruct<T>(byte[] bytes, int startIndex = 0)
    {
        if (bytes == null)
          return default(T);
        if (bytes.Length <= 0)
          return default(T);

        int objLength = Marshal.SizeOf(typeof(T));
        IntPtr bufferPtr = Marshal.AllocHGlobal(objLength);
        try//struct_bytes转换
        {
            Marshal.Copy(bytes, startIndex, bufferPtr, objLength);
            return (T)Marshal.PtrToStructure(bufferPtr, typeof(T));
        }
        catch(Exception ex)
        {
            throw new Exception("Error in BytesToStruct ! " + ex.Message);
        }
        finally
        {
            Marshal.FreeHGlobal(bufferPtr);
        }
    }
  }
}
