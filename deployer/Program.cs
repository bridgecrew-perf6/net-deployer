﻿namespace deployer;

internal static class Program
{
  public static void Main(string[] args)
  {
    var app = new Application();
    app.Copy();
  }
}