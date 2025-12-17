// Copyright (c) TFG Co. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
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

package health

import (
	"context"
	"fmt"
	"os"
	"path/filepath"
	"time"

	"github.com/nats-io/nats.go"
	"github.com/topfreegames/pitaya/v3/pkg/logger"
	clientv3 "go.etcd.io/etcd/client/v3"
)

const (
	LivenessCheckFile  = "/tmp/pitaya-health"
	ReadinessCheckFile = "/tmp/pitaya-ready"
)

type Checker struct{}

func NewChecker() *Checker {
	return &Checker{}
}

func (h *Checker) StartLivenessCheck() error {
	return h.createFile(LivenessCheckFile, "liveness")
}

func (h *Checker) StartReadinessCheck(timeout time.Duration, natsConn *nats.Conn, etcdCli *clientv3.Client) error {
	if natsConn != nil && !natsConn.IsConnected() {
		return fmt.Errorf("NATS is not connected")
	}

	if etcdCli != nil {
		endpoints := etcdCli.Endpoints()
		if len(endpoints) == 0 {
			return fmt.Errorf("ETCD has no endpoints configured")
		}

		checkTimeout := timeout
		if checkTimeout == 0 {
			checkTimeout = 5 * time.Second
		}

		ctx, cancel := context.WithTimeout(context.Background(), checkTimeout)
		defer cancel()

		_, err := etcdCli.Status(ctx, endpoints[0])
		if err != nil {
			return fmt.Errorf("ETCD is not reachable: %w", err)
		}
	}

	return h.createFile(ReadinessCheckFile, "readiness")
}

func (h *Checker) StopLivenessCheck() {
	_ = os.Remove(LivenessCheckFile)
}

func (h *Checker) StopReadinessCheck() {
	_ = os.Remove(ReadinessCheckFile)
	logger.Log.Info("readiness check stopped")
}

func (h *Checker) createFile(filename, content string) error {
	dir := filepath.Dir(filename)
	if err := os.MkdirAll(dir, 0o755); err != nil {
		return fmt.Errorf("failed to create %s directory: %w", filename, err)
	}

	file, err := os.OpenFile(filename, os.O_RDWR|os.O_CREATE|os.O_TRUNC, 0o644)
	if err != nil {
		return fmt.Errorf("failed to create %s file: %w", filename, err)
	}
	defer file.Close()

	_, err = file.Write([]byte(content))
	if err != nil {
		return fmt.Errorf("failed to write %s file: %w", filename, err)
	}

	return nil
}
