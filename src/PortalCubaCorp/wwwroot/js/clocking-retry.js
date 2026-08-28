/**
 * clocking-retry.js - Offline clocking retry mechanism (R006, AC-005).
 *
 * When the network is unavailable, clocking POST requests are stored in localStorage.
 * The mechanism retries every 10 seconds for up to 5 minutes.
 * If the network recovers within 5 minutes, the clocking is sent successfully.
 * An idempotency key prevents duplicate records on retry.
 * If 5 minutes elapse without recovery, the user sees a failure message.
 *
 * Acceptance Criteria (from PoC decision R006):
 *   1. Clocking POST stored in localStorage when network is unavailable
 *   2. Automatic retry every 10 seconds for up to 5 minutes
 *   3. Successful POST when network is restored within 5 minutes
 *   4. Idempotency key prevents duplicate records when the same clocking is retried
 *   5. Server accepts client-side timestamp
 *   6. User sees confirmation after successful retry
 *   7. User sees failure message if 5 minutes elapse without network recovery
 */
(function () {
    'use strict';

    var STORAGE_KEY = 'pendingClockings';
    var RETRY_INTERVAL_MS = 10000;  // 10 seconds
    var MAX_RETRY_DURATION_MS = 300000;  // 5 minutes
    var retryTimer = null;
    var retryStartTime = null;

    function generateIdempotencyKey(employeeId, timestamp) {
        return employeeId + '-' + timestamp + '-' + Math.random().toString(36).substr(2, 9);
    }

    function storePendingClocking(employeeId, timestamp, clockType) {
        var pending = {
            employeeId: employeeId,
            timestamp: timestamp,
            clockType: clockType,
            idempotencyKey: generateIdempotencyKey(employeeId, timestamp),
            storedAt: Date.now()
        };

        var allPending = getPendingClockings();
        allPending.push(pending);
        localStorage.setItem(STORAGE_KEY, JSON.stringify(allPending));
        return pending;
    }

    function getPendingClockings() {
        try {
            var raw = localStorage.getItem(STORAGE_KEY);
            return raw ? JSON.parse(raw) : [];
        } catch (e) {
            return [];
        }
    }

    function removePendingClocking(idempotencyKey) {
        var allPending = getPendingClockings();
        var filtered = allPending.filter(function (item) {
            return item.idempotencyKey !== idempotencyKey;
        });
        localStorage.setItem(STORAGE_KEY, JSON.stringify(filtered));
    }

    function sendPendingClocking(pending) {
        return fetch('/api/clocking', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                employeeId: pending.employeeId,
                timestamp: pending.timestamp,
                clockType: pending.clockType,
                idempotencyKey: pending.idempotencyKey
            })
        }).then(function (response) {
            if (response.ok) {
                removePendingClocking(pending.idempotencyKey);
                showConfirmation(pending);
                return true;
            }
            return false;
        }).catch(function () {
            return false;
        });
    }

    function showConfirmation(pending) {
        var message = document.createElement('div');
        message.className = 'clocking-confirmation';
        message.textContent = 'Clocking recorded successfully.';
        message.style.cssText = 'position:fixed;top:20px;right:20px;background:#d4edda;color:#155724;padding:12px 20px;border-radius:4px;z-index:9999;';
        document.body.appendChild(message);
        setTimeout(function () {
            message.remove();
        }, 5000);
    }

    function showFailureMessage() {
        var message = document.createElement('div');
        message.className = 'clocking-failure';
        message.textContent = 'Clocking failed - contact HR';
        message.style.cssText = 'position:fixed;top:20px;right:20px;background:#f8d7da;color:#721c24;padding:12px 20px;border-radius:4px;z-index:9999;';
        document.body.appendChild(message);
        setTimeout(function () {
            message.remove();
        }, 10000);
    }

    function startRetryLoop() {
        if (retryTimer) return;
        retryStartTime = Date.now();

        retryTimer = setInterval(function () {
            var elapsed = Date.now() - retryStartTime;
            if (elapsed >= MAX_RETRY_DURATION_MS) {
                stopRetryLoop();
                var pending = getPendingClockings();
                if (pending.length > 0) {
                    showFailureMessage();
                }
                return;
            }

            retryAllPending();
        }, RETRY_INTERVAL_MS);
    }

    function retryAllPending() {
        var allPending = getPendingClockings();
        if (allPending.length === 0) {
            stopRetryLoop();
            return;
        }

        Promise.all(allPending.map(function (pending) {
            return sendPendingClocking(pending);
        })).then(function (results) {
            if (getPendingClockings().length === 0) {
                stopRetryLoop();
            }
        });
    }

    function stopRetryLoop() {
        if (retryTimer) {
            clearInterval(retryTimer);
            retryTimer = null;
            retryStartTime = null;
        }
    }

    window.ClockingRetry = {
        submit: function (employeeId, timestamp, clockType) {
            var idempotencyKey = generateIdempotencyKey(employeeId, timestamp);
            var clocking = {
                employeeId: employeeId,
                timestamp: timestamp,
                clockType: clockType,
                idempotencyKey: idempotencyKey,
                storedAt: Date.now()
            };

            return fetch('/api/clocking', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(clocking)
            }).then(function (response) {
                if (response.ok) {
                    showConfirmation(clocking);
                    return { success: true, retried: false };
                }
                storePendingClocking(employeeId, timestamp, clockType);
                startRetryLoop();
                return { success: false, retried: true };
            }).catch(function () {
                storePendingClocking(employeeId, timestamp, clockType);
                startRetryLoop();
                return { success: false, retried: true };
            });
        },

        _internal: {
            generateIdempotencyKey: generateIdempotencyKey,
            storePendingClocking: storePendingClocking,
            getPendingClockings: getPendingClockings,
            removePendingClocking: removePendingClocking,
            startRetryLoop: startRetryLoop,
            stopRetryLoop: stopRetryLoop,
            retryAllPending: retryAllPending,
            RETRY_INTERVAL_MS: RETRY_INTERVAL_MS,
            MAX_RETRY_DURATION_MS: MAX_RETRY_DURATION_MS,
            STORAGE_KEY: STORAGE_KEY
        }
    };

    document.addEventListener('DOMContentLoaded', function () {
        var pending = getPendingClockings();
        if (pending.length > 0) {
            startRetryLoop();
        }
    });
})();