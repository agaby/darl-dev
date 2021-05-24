// helper method for displaying a status message.
const setMessage = (message) => {
    const messageDiv = document.querySelector('#messages');
    messageDiv.innerHTML += "<br>" + message;
}

// Fetch public key and initialize Stripe.
let stripe, cardElement;

stripe = Stripe(publishableKey);

const elements = stripe.elements();
cardElement = elements.create('card');
cardElement.mount('#card-element');

const form = document.querySelector('#subscribe-form');
form.addEventListener('submit', async (e) => {
    e.preventDefault();
    const nameInput = document.getElementById('name');

    // Create payment method and confirm payment intent.
    stripe.confirmCardPayment(clientSecret, {
        payment_method: {
            card: cardElement,
            billing_details: {
                name: nameInput.value,
            },
        }
    }).then((result) => {
        if (result.error) {
            setMessage(`Payment failed: ${result.error.message}`);
        } else {
            // Redirect the customer to their account page
            setMessage('Success! Redirecting to ThinkBase.');
            window.location.href = '/';
        }
    });
});
