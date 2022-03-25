


const launchDate = new Date("2022-04-01").getTime();

setInterval(test = () => {
    const date = Date.now();
    
    let difference = launchDate - date;
    
    const days = Math.floor(difference / 1000 / 60 / 60 / 24);
    document.getElementById("launch-days").innerText = days;
    
    difference -= days * 24 * 60 * 60 * 1000;
    
    const hours = Math.floor(difference / 1000 / 60 / 60);
    document.getElementById("launch-hours").innerText = hours;
    
    difference -= hours * 60 * 60 * 1000;
    
    const minutes = Math.floor(difference / 1000 / 60);
    document.getElementById("launch-minutes").innerText = minutes;
    
    difference -= minutes * 60 * 1000;
    
    
    const seconds = Math.floor(difference / 1000);
    document.getElementById("launch-seconds").innerText = seconds;
}, 1000);

test();
