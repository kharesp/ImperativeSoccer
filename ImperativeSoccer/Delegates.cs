using System;

namespace ImperativeSoccer
{
    public delegate void WorkerThreadStart<T>(WorkerThread<T> thread, object obj);
    public delegate void ExceptionHandler(object sender, Exception e);
    public delegate void ThreadProgress(object update); 
    public delegate void Demultiplexer<T>(T data);
}