// Copyright (c) TFG Co. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

package cluster

import (
	nats "github.com/nats-io/nats.go"
	clientv3 "go.etcd.io/etcd/client/v3"
)

// GetNatsConn returns the NATS connection from NatsRPCClient (for health checks)
func (ns *NatsRPCClient) GetNatsConn() *nats.Conn {
	return ns.conn
}

// GetNatsConn returns the NATS connection from NatsRPCServer (for health checks)
func (ns *NatsRPCServer) GetNatsConn() *nats.Conn {
	return ns.conn
}

// GetEtcdClient returns the ETCD client from etcdServiceDiscovery (for health checks)
func (sd *etcdServiceDiscovery) GetEtcdClient() *clientv3.Client {
	return sd.cli
}
