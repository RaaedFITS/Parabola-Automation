import sys
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.action_chains import ActionChains
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import time

# Get the flow name and file path from command-line arguments
if len(sys.argv) < 3:
    print("Please provide the flow name and file path as arguments.")
    sys.exit(1)

flow_name = sys.argv[1]
file_path = sys.argv[2]
print(f"Flow name: {flow_name}")
print(f"File path: {file_path}")

# Set up the Chrome WebDriver
print("Initializing WebDriver...")
browser = webdriver.Chrome()
print("Maximizing browser window...")
browser.maximize_window()

try:
    # Open the Parabola login page
    print("Navigating to Parabola login page...")
    browser.get('https://parabola.io/app/flows/home')
    browser.implicitly_wait(2)
    print("Page loaded successfully.")

    # Log in to Parabola using email and password
    print("Using email and password login...")
    email_field = browser.find_element(By.XPATH, "//input[@placeholder='you@company.com']")
    password_field = browser.find_element(By.XPATH, "//input[@type='password']")
    login_button = browser.find_element(By.XPATH, "//button[contains(text(), 'Sign in')]")
    email = "rbucksimiar@gmail.com"
    password = "Raaed123#"
    email_field.send_keys(email)
    password_field.send_keys(password)
    login_button.click()
    print("Sign-in form submitted.")

    # Wait for the dashboard to load
    print("Waiting for the dashboard to load...")
    time.sleep(2)

    # Locate the flow link dynamically using the flow name
    print(f"Looking for the '{flow_name}' link...")
    flow_link = browser.find_element(By.XPATH, f"//a[text()='{flow_name}']")
    flow_link.click()
    print(f"Clicked on the '{flow_name}' link.")
    time.sleep(2)

    # Locate the deepest interactive element
    print("Locating the deeper inner element to click...")
    deepest_element = browser.find_element(By.XPATH, "//div[@class='css-1q4vvn7']")
    print("Performing a click on the deeper element...")
    actions = ActionChains(browser)
    actions.click(deepest_element).perform()
    time.sleep(1)
    print("Click on the deeper element completed.")

    # Perform a double-click action to ensure interaction
    print("Performing a double-click on the deeper element...")
    actions.double_click(deepest_element).perform()
    print("Double-click on the deeper element completed.")
    time.sleep(3)

    # Locate the file upload input element dynamically
    print("Locating the file upload input element...")
    file_input = browser.find_element(By.XPATH, "//div[contains(@class, 'css-1i2ya67')]//input[@type='file']")
    print(f"Uploading file: {file_path}")
    file_input.send_keys(file_path)
    print("File uploaded successfully.")
    time.sleep(5)

    # Locate and click the Exit button
    print("Looking for the 'Exit' button...")
    try:
        exit_button = browser.find_element(By.XPATH, "//div[contains(@class, 'modal-dismiss')]")
        exit_button.click()
        print("Clicked on the 'Exit' button.")
        time.sleep(2)
    except Exception as e:
        print(f"Exit button not found or could not be clicked: {e}")

    # Wait for the first "Run Flow" button to become clickable
    print("Waiting for the first 'Run Flow' button to become clickable...")
    wait = WebDriverWait(browser, 30)  # Wait for up to 30 seconds
    first_run_flow_button = wait.until(
        EC.element_to_be_clickable((By.XPATH, "//button[contains(@class, 'css-1eo098y') and contains(text(), 'Run Flow')]"))
    )
    print("The first 'Run Flow' button is now clickable.")
    first_run_flow_button.click()
    print("Clicked on the first 'Run Flow' button.")
    time.sleep(5)

    # Wait for the second "Run Flow" button to appear and click it
    print("Waiting for the second 'Run Flow' button to become visible...")
    second_run_flow_button = wait.until(
        EC.element_to_be_clickable((By.XPATH, "//div[@class='css-1aj1cwm el2um1b0']//div[contains(@class, 'css-1yzkv2u') and text()='Run Flow']"))
    )
    print("The second 'Run Flow' button is now clickable.")
    second_run_flow_button.click()
    print("Clicked on the second 'Run Flow' button.")
    time.sleep(5)

    # Wait for the "Run Flow" button to return to its initial state
    print("Waiting for the 'Run Flow' button to return to its initial state...")
    wait.until(
        EC.presence_of_element_located((By.XPATH, "//button[contains(@class, 'css-hgd1m') and contains(text(), 'Run Flow')]"))
    )
    print("The 'Run Flow' button has returned to its initial state.")

except Exception as e:
    print(f"An error occurred: {e}")
finally:
    # Close the browser
    print("Closing the browser...")
    browser.quit()
    print("Browser closed.")
