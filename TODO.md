# TODO

* Make TaskWorker for general usage, which supports custom queue message handler in any programming language
* Make TaskMonitor, which supports
  * Creating containerized workers in k8s
  * More scaling policies
* Improve TaskQueueServer
  * Support more concurrent REST API calls (> 100) (may need more connections to PostgreSQL)
  * Support authentication
  * [Optionally] Return message Id when putting a message in queue
* Improve TaskQueueClient
  * Support retry on failure of REST API call
  * Accept HttpClient as a parameter in CTOR
