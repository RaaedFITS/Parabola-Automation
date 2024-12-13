import json
from selenium import webdriver
from selenium.webdriver.common.by import By
import time
import os

SESSION_FILE = "flows.json"
POLL_INTERVAL = 5  # Time in seconds between checks

# Global browser instance
browser = None

def start_browser(headless=False):
    """Start and return a persistent Selenium browser instance."""
    global browser
    if browser is None:
        options = webdriver.ChromeOptions()
        options.add_argument("--start-maximized")  # Open maximized
        options.add_argument("--disable-extensions")
        
        if headless:
            options.add_argument("--headless")  # Run in headless mode
            options.add_argument("--disable-gpu")  # Disable GPU acceleration
            options.add_argument("--window-size=1920,1080")  # Set a default window size for headless mode
        
        browser = webdriver.Chrome(options=options)
        browser.implicitly_wait(10)
        browser.get('https://parabola.io/app/flows/home')

        # Login (if needed)
        try:
            email = "rbucksimiar@gmail.com"
            password = "Raaed123#"
            email_field = browser.find_element(By.XPATH, "//input[@placeholder='you@company.com']")
            password_field = browser.find_element(By.XPATH, "//input[@type='password']")
            login_button = browser.find_element(By.XPATH, "//button[contains(text(), 'Sign in')]")
            email_field.send_keys(email)
            password_field.send_keys(password)
            login_button.click()
            time.sleep(5)  # Wait for login to complete
        except Exception as e:
            print("Login error (likely already logged in):", e)

    return browser

def fetch_flows():
    """Fetch the current flow names from the dashboard."""
    try:
        flow_elements = browser.find_elements(By.XPATH, "//a[contains(@class, 'css-2ht1md')]")
        flow_names = [flow.text.strip() for flow in flow_elements if flow.text.strip()]
        return flow_names
    except Exception as e:
        print(f"Error fetching flows: {e}")
        return []

def get_flows_from_file():
    """Read flow names from the session file."""
    if os.path.exists(SESSION_FILE):
        try:
            with open(SESSION_FILE, "r") as file:
                return json.load(file)
        except json.JSONDecodeError as e:
            print(f"Error decoding JSON file: {e}")
    return []

def save_flows_to_file(flows):
    """Save flow names to the session file."""
    with open(SESSION_FILE, "w") as file:
        json.dump(flows, file)

def monitor_dashboard(headless=False):
    """Continuously monitor the dashboard for changes."""
    global browser
    browser = start_browser(headless=headless)
    existing_flows = set(get_flows_from_file())

    try:
        while True:
            # Reload the page
            print("Reloading the dashboard...")
            browser.refresh()
            time.sleep(3)  # Allow time for the page to reload

            # Fetch updated flows
            new_flows = set(fetch_flows())

            if new_flows != existing_flows:
                print(f"Flows updated: {new_flows}")
                save_flows_to_file(list(new_flows))
                existing_flows = new_flows
            else:
                print("No changes detected.")

            time.sleep(POLL_INTERVAL)  # Wait before polling again
    except KeyboardInterrupt:
        print("Stopping dashboard monitoring.")
    finally:
        if browser:
            browser.quit()

if __name__ == "__main__":
    # Pass `headless=True` to run in headless mode
    monitor_dashboard(headless=True)
