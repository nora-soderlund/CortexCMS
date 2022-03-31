let launch = document.getElementById("launch-input").value;

setInterval(test = () => {
    const date = Date.now();

    let seconds = launch;
    
    const days = Math.floor(seconds / 60 / 60 / 24);
    document.getElementById("launch-days").innerText = days;
    
    seconds -= days * 24 * 60 * 60;
    
    const hours = Math.floor(seconds / 60 / 60);
    document.getElementById("launch-hours").innerText = hours;
    
    seconds -= hours * 60 * 60;
    
    const minutes = Math.floor(seconds / 60);
    document.getElementById("launch-minutes").innerText = minutes;
    
    seconds -= minutes * 60;
    
    document.getElementById("launch-seconds").innerText = Math.floor(seconds);

    launch--;
}, 1000);

test();
