module Infrastructure

open System.Threading

let serverCancellationTokenSource = new CancellationTokenSource()