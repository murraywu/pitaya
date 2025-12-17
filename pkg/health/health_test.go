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
	"os"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

func TestNewChecker(t *testing.T) {
	t.Parallel()

	checker := NewChecker()
	assert.NotNil(t, checker)
}

func TestChecker_Liveness(t *testing.T) {
	checker := NewChecker()
	require.NotNil(t, checker)

	// Clean up before test
	_ = os.Remove(LivenessCheckFile)

	// Start should create the liveness file
	err := checker.StartLivenessCheck()
	require.NoError(t, err)

	// File should exist
	_, err = os.Stat(LivenessCheckFile)
	require.NoError(t, err)

	// Stop should remove the liveness file
	checker.StopLivenessCheck()

	// File should not exist after stop
	_, err = os.Stat(LivenessCheckFile)
	assert.True(t, os.IsNotExist(err))
}

func TestChecker_DoubleLiveness(t *testing.T) {
	checker := NewChecker()
	require.NotNil(t, checker)

	// Clean up before test
	_ = os.Remove(LivenessCheckFile)

	err := checker.StartLivenessCheck()
	require.NoError(t, err)
	defer checker.StopLivenessCheck()

	// Starting again should not error
	err = checker.StartLivenessCheck()
	require.NoError(t, err)
}

func TestChecker_StopWithoutStart(t *testing.T) {
	checker := NewChecker()
	require.NotNil(t, checker)

	// Stopping without starting should not panic
	checker.StopLivenessCheck()
	checker.StopReadinessCheck()
}

func TestChecker_Readiness_Standalone(t *testing.T) {
	// Standalone mode: no NATS/ETCD connections
	checker := NewChecker()
	require.NotNil(t, checker)

	// Clean up before test
	_ = os.Remove(ReadinessCheckFile)

	// Should work without connections (pass nil)
	err := checker.StartReadinessCheck(5*time.Second, nil, nil)
	require.NoError(t, err)

	// File should exist
	_, err = os.Stat(ReadinessCheckFile)
	require.NoError(t, err)

	// Clean up
	checker.StopReadinessCheck()

	// File should not exist after stop
	_, err = os.Stat(ReadinessCheckFile)
	assert.True(t, os.IsNotExist(err))
}

func TestChecker_Readiness_ZeroTimeout(t *testing.T) {
	// Test with zero timeout - should use default
	checker := NewChecker()
	require.NotNil(t, checker)

	// Clean up before test
	_ = os.Remove(ReadinessCheckFile)

	// Should work with zero timeout (uses default 5s)
	err := checker.StartReadinessCheck(0, nil, nil)
	require.NoError(t, err)

	// File should exist
	_, err = os.Stat(ReadinessCheckFile)
	require.NoError(t, err)

	// Clean up
	checker.StopReadinessCheck()
}
