﻿$path = Split-Path $MyInvocation.MyCommand.Path;
cd $path

.\ComputationalCluster.CommunicationServer.Console\bin\Debug\ComputationalCluster.CommunicationServer.Console.exe


$block = {& ".\ComputationalCluster.CommunicationServer.Console\bin\Debug\ComputationalCluster.CommunicationServer.Console.exe" }
$job = start-job -scriptblock $block