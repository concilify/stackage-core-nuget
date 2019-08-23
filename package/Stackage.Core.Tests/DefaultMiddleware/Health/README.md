Would like to test client_cancels_request but can't get past System.IO.IOException : The request was aborted or the pipeline has finished. Googling points to this https://github.com/aspnet/Hosting/issues/320

Would like to implement the following, but sealed HealthReport calculates aggregate making this difficult.

          Critical      Important    Cosmetic
Healthy   Healthy/200   Healthy/200  Healthy/200
Degraded  Degraded/200  Healthy/200  Healthy/200 
Unhealthy Unhealthy/503 Degraded/200 Healthy/200

Healthy - all functions working as designed
Degraded - can respond to requests - must not lose requests/data
Unhealthy - unable to respond to requests without potential data loss
