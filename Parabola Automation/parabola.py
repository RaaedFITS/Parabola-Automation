import sys
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.action_chains import ActionChains
import time

# Get the flow name from the command-line argument
if len(sys.argv) < 2:
    print("Please provide the flow name as an argument.")
    sys.exit(1)

flow_name = sys.argv[1]
print(f"Flow name received: {flow_name}")

# Set up the Chrome WebDriver
print("Initializing WebDriver...")
browser = webdriver.Chrome()

try:
    # Open the Parabola login page
    print("Navigating to Parabola login page...")
    browser.get('https://parabola.io/app/flows/home')
    browser.implicitly_wait(2)
    print("Page loaded successfully.")

    # Log in to Parabola using email and password
    print("Using email and password login...")
    email_field = browser.find_element(By.XPATH, "//input[@placeholder='you@company.com']")  # Locate the email field by its placeholder
    password_field = browser.find_element(By.XPATH, "//input[@type='password']")  # Locate the password field by its type
    login_button = browser.find_element(By.XPATH, "//button[contains(text(), 'Sign in')]")  # Locate the sign-in button by its text
    # CREDENTIALS
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
    time.sleep(2)  # Wait for navigation to the next page

    # Locate the deepest interactive element
    print("Locating the deeper inner element to click...")
    deepest_element = browser.find_element(By.XPATH, "//div[@class='css-1q4vvn7']")  # Refined XPath for deeper targeting

    # Perform the click action
    print("Performing a click on the deeper element...")
    actions = ActionChains(browser)
    actions.click(deepest_element).perform()
    time.sleep(1)
    print("Click on the deeper element completed.")

    # Perform a double-click action to ensure interaction
    print("Performing a double-click on the deeper element...")
    actions.double_click(deepest_element).perform()
    print("Double-click on the deeper element completed.")

    # Delay before ending the script to observe changes
    time.sleep(3)

    # Locate the file upload input element dynamically
    print("Locating the file upload input element...")
    file_input = browser.find_element(By.XPATH, "//div[contains(@class, 'css-1i2ya67')]//input[@type='file']")

    # Click on the file upload input using ActionChains
    print("Clicking the file upload input element...")
    actions = ActionChains(browser)
    actions.move_to_element(file_input).click().perform()
    print("File upload input clicked.")
    time.sleep(10)

    # Upload a file
    file_path = r"C:\\Users\\developer_fitsexpres\\Downloads\\SampleCSVFile_2kb.csv"  # Replace with the correct file path
    print(f"Uploading file: {file_path}")
    file_input.send_keys(file_path)
    print("File uploaded successfully.")

    time.sleep(10)
except Exception as e:
    print(f"An error occurred: {e}")
finally:
    # Close the browser
    print("Closing the browser...")
    browser.quit()
    print("Browser closed.")
