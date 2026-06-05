# OLED Sleeper 😴 – Blackout or Dim Secondary Monitors on Windows

Added -e to exit, -p to pause and -s to resume. Should also stop dimming monitor while blacking out. Fixed security issue with the test. Vibe coded, but seems working fine.
-b1 to force blackout the 1st monitor, b2 - same for the 2nd (can't be woken up); -e1 and -e2 to enable again.
-g for 400 ms delay to avoid waking up in games with raw input and -w for 0 ms delay for work.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

OLED Sleeper is a lightweight Windows tool to blackout or dim idle monitors, helping users prevent OLED burn-in and temporarily sleep secondary monitors for focus, gaming, or distraction-free work.

<p align="center">
  <img src="https://github.com/user-attachments/assets/0f7c9110-094c-4fdb-8109-62fdd11e87cd" alt="OLED Sleeper Demonstration"> 
</p>

---

## The Problem

Many users have multi-monitor setups but want to turn off or dim secondary monitors temporarily without putting the entire computer to sleep. OLED and other displays can also suffer from burn-in or image retention if static images stay on screen too long. Windows’ built-in power settings are all-or-nothing — there’s no per-monitor control.

## The Solution

OLED Sleeper monitors each screen for activity. When a monitor is idle for a set time, it will either black it out or dim its brightness based on your preference. 

<p align="center">
  <img width="500" height="410" alt="oled-sleeper-002" src="https://github.com/user-attachments/assets/20022234-fe5f-4573-a4e4-ff5ef07b622b" />
</p>

---

## Features

* **Three Idle Detection Modes:** Customize how the application determines if a monitor is idle:
    * **Mouse:** Tracks cursor movement specifically on the target monitor.
    * **Focused Application:** Tracks activity within the active window currently displayed on that monitor.
    * **System-Wide Input:** Tracks overall keyboard and mouse input across the entire system (similar to standard Windows idle detection).
* **Per-Monitor Control:** Blackout or dim any monitor independently.
* **Two Action Modes:** Full blackout or dimming (DDC/CI supported).
* **Instant Wake-Up:** Restore the monitor immediately when activity is detected.
* **Native WPF Application:** Built from the ground up using native Win32 calls. Requires no external dependencies or third-party tools.

---

## Requirements

* **Operating System:** Windows 10 or 11
* **DDC/CI Support (for Dimming Mode):** Dimming requires a monitor that supports DDC/CI brightness control via VCP codes. Most modern monitors support this, but it is not guaranteed on all displays.

---

## How to Use

Download the latest release from the [Releases page](https://github.com/Ghokury/OLED-Sleeper).

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
