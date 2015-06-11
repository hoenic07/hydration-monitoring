function [] = h2o()
global allAngleResults
global allCalculatedHelpValues
global allDrinkTimes
global allActualDrinks
global allCalculatedDrinks
global allWekaAnalysisData

% read all files
fnames = dir('<PATH>\current\*.txt');
folder='<PATH>\current\';
numfids = length(fnames);

allActualDrinks=[];
allCalculatedDrinks=[];

allDrinkTimes=[];
allAngleResults=[];
allCalculatedHelpValues=[];
allWekaAnalysisData=[];

close all

for K = 1:2:numfids
    
  % take two files: the first is are the sensor values, the second one the
  % actual drink amounts
  fn1 = fnames(K);
  fn2 = fnames(K+1);
  disp(strcat({'Test file: '},fn1.name));
  sensorVals = load(strcat(folder,fn1.name));
  actualDrinkAmount= load(strcat(folder,fn2.name))';
  actualDrinkAmountNotCumulative=uncumulateDrinkAmounts(actualDrinkAmount);
  
  calculateDrinkAmount(sensorVals,actualDrinkAmountNotCumulative);
  
  allActualDrinks=vertcat(allActualDrinks, actualDrinkAmountNotCumulative);
  
  %plotAngles(sensorVals);
  
  if length(allActualDrinks) ~= length(allCalculatedHelpValues)
      disp('Error');
  end
  
end

%shuffle
if(true)
    drinkAmounts=length(allActualDrinks);
    shuffleIndex = [24,15,14,26,31,39,34,33,5,28,32,16,35,29,44,6,2,38,41,21,37,27,4,18,12,20,22,8,17,43,30,1,7,23,9,25,42,36,19,3,11,13,40,10;];
    %rough test indeces
    %[21,19,43,38,64,35,17,7,12,26,5,23,37,25,67,22,48,20,30,50,15,9,6,34,24,10,55,60,68,4,51,32,3,18,54,2,11,8,40,36,62,31,13,33,65,47,59,27,56,46,44,45,14,66,42,49,16,1,61,53,52,28,63,58,57,29,39,41];
    %detailed test indeces
    %[24,15,14,26,31,39,34,33,5,28,32,16,35,29,44,6,2,38,41,21,37,27,4,18,12,20,22,8,17,43,30,1,7,23,9,25,42,36,19,3,11,13,40,10;]
    %randperm(drinkAmounts);
    allWekaAnalysisData=allWekaAnalysisData(shuffleIndex,:);
    allActualDrinks=allActualDrinks(shuffleIndex,:);
    allCalculatedHelpValues=allCalculatedHelpValues(shuffleIndex,:);
end

if(false)
    figure
    hold on;
    showTimeAmountDiagram(allActualDrinks,allCalculatedHelpValues);
    hold off;
end


angleAnalysis(allAngleResults);
%showWekaRegressionResult(allWekaAnalysisData);

end

%shows one diagram that contains all drinks
function [maxDiffPercent] = showTimeAmountDiagram(actualAmounts, calculatedValues)

totalSets=length(actualAmounts);
testBegin = int32(totalSets*0.66);
%idx_rand =randperm(totalSets);
trainingIndeces=1:testBegin-1;
testIndeces=testBegin:totalSets;

%use all instead
if(true)
    trainingIndeces=1:totalSets;
    testIndeces=1:totalSets;
end

%define training and test sets
trainingSetAmounts=actualAmounts(trainingIndeces);
trainingSetValues=calculatedValues(trainingIndeces);
testSetAmounts=actualAmounts(testIndeces);
testSetValues=calculatedValues(testIndeces);

overall=zeros(totalSets-testBegin,5);

for i=1:length(testSetAmounts)  
    overall(i,:)=[testSetAmounts(i) testSetValues(i) 0 0 0];
end

maxV=max(trainingSetValues);
%p = polyfit(trainingSetValues,trainingSetAmounts,1)
p=[0.103679754347243,17.4534847257996;]; % detailed test
%p = [0.0646793974875693,2.67453653350907;] % rough test
for i=1:length(testSetValues)  
    calcedVal=polyval(p,testSetValues(i));
    overall(i,3)=calcedVal;
    overall(i,4)=calcedVal-overall(i,1);
    overall(i,5)=abs(calcedVal-overall(i,1));
end

scatter(overall(:,1), overall(:,3));
xlabel('Actual volumes (ml)')
ylabel('Calculated volumes (ml)')
plot(1:150,1:150);
title('Errors of calculated volumes (Matlab)');

sumAct=sum(overall(:,1))
sumCalc=sum(overall(:,3))
diff=sum(overall(:,4))
diffAbs=sum(overall(:,5))
maxDiffPercent=(diffAbs/sumAct)*100

end

function showWekaRegressionResult(weka)

rows=length(weka);
overall=zeros(rows,5);

for i=1:rows 
    % set variables that have the same name as in the weka function
    smoothCurveLength=weka(i,1);
    variance = weka(i,2);
    standardDeviationOfAngles=weka(i,4);
    slopeDuringDrinking=weka(i,3);
    time=weka(i,5);
    sov=weka(i,6);

    % rough test formula
    %calculatedAmount = 0.1361 * smoothCurveLength + 13.1571 * variance + -1.0258 * standardDeviationOfAngles + 0.0036 * time + -7.9756;
    calculatedAmount = 13.5846 * variance + 0.0099 * time + 6.4219;
    overall(i,1)=weka(i,7);
    overall(i,3)=calculatedAmount;
    overall(i,4)=calculatedAmount-overall(i,1);
    overall(i,5)=abs(calculatedAmount-overall(i,1));
end

figure;
hold on;
scatter(overall(:,1), overall(:,3));
xlabel('Actual volumes (ml)')
ylabel('Calculated volumes (ml)')
plot(1:150,1:150);
title('Errors of calculated volumes (WEKA)');

hold off;

disp('WEKA');
sumAct=sum(overall(:,1))
sumCalc=sum(overall(:,3))
diff=sum(overall(:,4))
diffAbs=sum(overall(:,5))
maxDiffPercent=(diffAbs/sumAct)*100

end

%Compares the calculated drink amount with the actual ones and prints the result%
function [] = compareDrinkAmounts(calculated, actual)

lenC = length(calculated);
lenA = length(actual);

if(lenC ~= lenA)
    disp('Actual/Calculated arrays have not same length. Calc total:');
    total = sum(actual);
    totalC = sum(calculated);
    totalDiff=totalC-total;
    pr('Total (Actual): ', total,'ml');
    pr('Total (Calculated): ', totalC,'ml');
    pr('TotalDiff: ', totalDiff,'ml');
    pr('Percentage: ', (totalDiff/total)*100,'%');
    return; 
end

total=0;
totalC=0;
totalDiff=0;


for i=1:lenC
    totalC=totalC+calculated(i);
    total=total+actual(i);
    diff = abs(actual(i)-calculated(i));
    totalDiff=totalDiff+diff;
    %[actual(i) calculated(i) diff]
end

pr('Total (Actual): ', total,'ml');
pr('Total (Calculated): ', totalC,'ml');
pr('TotalDiff: ', totalDiff,'ml');
pr('Percentage: ', (totalDiff/total)*100,'%');

end

%Calculation for one drink file
function calculateDrinkAmount(sensorVals, drinkAmountsActual)

len = length(sensorVals);
fa=10;%hz
tbr=100; %ms
overall=sqrt(sensorVals(:,1).^2+sensorVals(:,2).^2+sensorVals(:,3).^2) - 1;

if(false)
    figure
    subplot(4,1,1)
    plot(sensorVals(:,1));
    title('X');
    subplot(4,1,2)
    plot(sensorVals(:,2));
    title('Y');
    subplot(4,1,3)
    plot(sensorVals(:,3));
    title('Z');
    subplot(4,1,4)
    plot(overall);
    title('sqrt(X^2 + Y^2 + Z^2)');
end

% y axis gives best results according to plots. Sensor is attached on the
% right side of the bottle

% detect rising an falling edges

% smooth values with +/- 5 vals avg
smoothedVals = smoothValues(sensorVals(:,2), 5); 

threshold=-0.2; % haeferl -0,75; red bull (250ml) -0.6; aladdin bottle (600ml) -0.25/-0.18 % 0.5l pet:
edges=zeros(0, 3);
badEdges=[];
badEdgeCount=0;
hasFall=false;
edgeCount=0;

for i=1:len-1
    y=smoothedVals(i);
    y1=smoothedVals(i+1);
    
    if(hasFall==false)
        if(y1>y&&y>=threshold)
            edgeCount=edgeCount+1;
            edges(edgeCount,:)=[i, 0, 0];
            hasFall=true;
        end
    else 
        if(y1<y&&y<=threshold)
            edges(edgeCount,2)=i;
            edges(edgeCount,3)=(i-edges(edgeCount,1))*tbr - 6*tbr;
            hasFall=false;
        end
    end
        
end

for i=1:edgeCount
    range=edges(i,1):edges(i,2);
    badEdgeCount=badEdgeCount+1;
    badEdges(badEdgeCount,:)=[double(int32((edges(i,2)-edges(i,1))/2)+edges(i,1)), drinkAmountsActual(i)/100];
    analyzeValuesDuringDrink(sensorVals(range,2), smoothedVals(range), drinkAmountsActual(i), tbr, overall(range));
end

% print y-axis, smoothed values and edges
if (false)
    figure
    hold on
    plot(smoothedVals,'r');
    
    plot(sensorVals(:,2));
    scatter(edges(:,1),ones(edgeCount,1)*threshold);
    scatter(edges(:,2),ones(edgeCount,1)*threshold);
    %scatter(badEdges(:,1),badEdges(:,2));
    hold off
    legend('Smoothed sensor values (windows: +/-5)','Raw sensor values','Rising edge','Falling edge');
end


end

function plotAngles(sensorVals)
angles=real(rad2deg(asin(sensorVals(:,2)*1.11)));
figure;
plot(angles);
end

function angleAnalysis(angles)
    disp('Angles used:')
    meanAngle=mean(angles)
    stdAngle=std(angles)
end

%exact analyzation of one drink
function analyzeValuesDuringDrink(values, smoothVals, drinkAmountActual, tbr, overallVals)
    global allAngleResults
    global allCalculatedHelpValues
    global allDrinkTimes
    global allCalculatedDrinks
    global allWekaAnalysisData
    
    len = length(values);
    
    % calculate Angles
    angles = real(rad2deg(asin(values*1.11)));
    smoothAngles=smoothValues(angles, 5);
    dist=0;
    finalDist=0;
    distSmooth=0;
    distAllAxes=0;
    for i=1:len-1
        dist=dist+ sqrt((angles(i)-angles(i+1))^2+1);
        distAllAxes=distAllAxes+sqrt((overallVals(i)-overallVals(i+1))^2+1);
        distSmooth=distSmooth+sqrt((smoothAngles(i)-smoothAngles(i+1))^2+1);
    end
    
    % check if and how much the bottle is rising during drinking
    center=int32(len/2);
    sh=smoothAngles(center:len);
    fh=smoothAngles(1:center);
    diffAng = (mean(sh)-mean(fh));
    
    % check how much value does change in central block
    begCT=int32(len/3);
    endCT=int32(2*len/3);
    centralAngles=angles(begCT:endCT);
    meanCA=mean(centralAngles);
    stdCA=std(centralAngles);
    
    %calc variance
    variance = values - smoothVals;
    sumVar = sum(abs(variance));
    cvs = localVariance(values, smoothVals,5);
    correctVariance=sum(cvs);
    
    
    %use all sensor vals in the range
    overallSmoothVals=smoothValues(overallVals,5);
    overallVariance=overallVals - overallSmoothVals;
    sov = 100000*((sum(overallVariance)/len)^2);
    
    avgAngle = mean(centralAngles)
    allAngleResults=vertcat(allAngleResults, avgAngle);
    
    
    drinkTime=(len-6)*tbr;
    allDrinkTimes=vertcat(allDrinkTimes, drinkTime);
    
    finalDist=2.5*distSmooth-(0.55*diffAng)^2 +50*sumVar + 5*len+ 2.5*stdCA;
    % finalDist=2.5*distSmooth-(0.55*diffAng)^2 +50*sumVar + 5*len+
    % 2.5*stdCA; -> For me!
    allCalculatedHelpValues=vertcat(allCalculatedHelpValues, finalDist);
    
    wekaData=[distSmooth, sumVar, diffAng, stdCA, len*tbr, sov,  drinkAmountActual];
    allWekaAnalysisData=vertcat(allWekaAnalysisData, wekaData);
    
    
    % actual calcualtion for one drink
    p=[0.444748292224150,-3.01479624488542];
    drinkAmount= polyval(p, finalDist);
    allCalculatedDrinks=vertcat(allCalculatedDrinks, drinkAmount);
    
    if abs(drinkAmount-drinkAmountActual) >30 && false
        figure
        
%         subplot(2,1,1);
%         plot(cAng);
        
        subplot(1,1,1);
        hold on
        plot(values)
        plot(smoothVals,'r')
        hold off
        title(num2str(drinkAmount-drinkAmountActual));
        
    end
end




%Help function to display text and a number with a unit%
function pr(text, number, unit)
 disp(strcat({text},num2str(number),unit));
end

function [result] = smoothValues(values, windowsize)

len = length(values);
result = zeros(len,1);

for i=1:len
    left=windowsize;
    right=windowsize;
    
    if(i<=windowsize)
        left=i-1;
    end
    
    if(i>=len-windowsize)
        right=len-i;
    end
    va=values(i-left:i+right);
    s = sum(va);
    result(i) = s / (left+right+1); 
end

end

function [result] = localVariance(values1, values2, windowsize)

len = length(values1);
result = zeros(len,1);

for i=1:len
    left=windowsize;
    right=windowsize;
    
    if(i<=windowsize)
        left=i-1;
    end
    
    if(i>=len-windowsize)
        right=len-i;
    end
    
    sumBoth = (sum(values1(i-left:i+right))-sum(values2(i-left:i+right))).^2;
    
    result(i) = sumBoth / (left+right+1); 
end

end

function [result] = uncumulateDrinkAmounts(cumulative)
   cumuLast=0;
   result=zeros(length(cumulative),1);
   for i=1:length(cumulative)
       result(i)=cumulative(i)-cumuLast;
       cumuLast=cumulative(i);
   end
   
end
